using Processor.Worker.Models;
using Processor.Worker.Providers;
using System.Diagnostics;
using System.Text.Json;

namespace Processor.Worker.Services;

public class NotificationProcessor : INotificationProcessor
{
    private readonly ILogger<NotificationProcessor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public NotificationProcessor(
        ILogger<NotificationProcessor> logger,
        IServiceProvider serviceProvider,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ProcessingResult> ProcessNotificationAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Iniciando processamento da notificação {NotificationId} para usuário {UserId} via canal {Channel}", 
                message.Id, message.UserId, message.Channel);

            // 1. Validar notificação
            var isValid = await ValidateNotificationAsync(message, cancellationToken);
            if (!isValid)
            {
                var validationResult = new ProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Notificação inválida ou não autorizada",
                    ProcessingTime = stopwatch.Elapsed
                };

                await UpdateNotificationStatusAsync(message.Id, "failed", validationResult, cancellationToken);
                return validationResult;
            }

            // 2. Verificar se está agendada para o futuro
            if (message.ScheduledFor.HasValue && message.ScheduledFor.Value > DateTime.UtcNow)
            {
                _logger.LogInformation("Notificação {NotificationId} agendada para {ScheduledFor}, pulando processamento", 
                    message.Id, message.ScheduledFor.Value);

                return new ProcessingResult
                {
                    Success = true,
                    ErrorMessage = "Notificação agendada para o futuro",
                    ProcessingTime = stopwatch.Elapsed
                };
            }

            // 3. Atualizar status para "processing"
            await UpdateNotificationStatusAsync(message.Id, "processing", null, cancellationToken);

            // 4. Obter provedor apropriado
            var provider = GetProviderForChannel(message.Channel);
            if (provider == null)
            {
                var providerResult = new ProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"Provedor não encontrado para o canal '{message.Channel}'",
                    ProcessingTime = stopwatch.Elapsed
                };

                await UpdateNotificationStatusAsync(message.Id, "failed", providerResult, cancellationToken);
                return providerResult;
            }

            // 5. Verificar saúde do provedor
            var isHealthy = await provider.IsHealthyAsync(cancellationToken);
            if (!isHealthy)
            {
                var healthResult = new ProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"Provedor {provider.Channel} não está saudável",
                    ProcessingTime = stopwatch.Elapsed
                };

                await UpdateNotificationStatusAsync(message.Id, "failed", healthResult, cancellationToken);
                return healthResult;
            }

            // 6. Processar notificação
            var result = await provider.SendAsync(message, cancellationToken);
            
            // 7. Atualizar status final
            var finalStatus = result.Success ? "sent" : "failed";
            await UpdateNotificationStatusAsync(message.Id, finalStatus, result, cancellationToken);

            _logger.LogInformation("Processamento da notificação {NotificationId} concluído com status {Status} em {ProcessingTime}ms", 
                message.Id, finalStatus, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar notificação {NotificationId}", message.Id);
            
            var errorResult = new ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTime = stopwatch.Elapsed
            };

            try
            {
                await UpdateNotificationStatusAsync(message.Id, "failed", errorResult, cancellationToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Erro ao atualizar status da notificação {NotificationId}", message.Id);
            }

            return errorResult;
        }
    }

    public async Task<bool> ValidateNotificationAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Validando notificação {NotificationId} para usuário {UserId}", message.Id, message.UserId);

            // 1. Validações básicas
            if (string.IsNullOrEmpty(message.Id) || 
                string.IsNullOrEmpty(message.UserId) || 
                string.IsNullOrEmpty(message.Channel))
            {
                _logger.LogWarning("Notificação {NotificationId} possui campos obrigatórios vazios", message.Id);
                return false;
            }

            // 2. Verificar se o canal é suportado
            var supportedChannels = new[] { "email", "sms", "push" };
            if (!supportedChannels.Contains(message.Channel.ToLower()))
            {
                _logger.LogWarning("Canal {Channel} não é suportado para notificação {NotificationId}", 
                    message.Channel, message.Id);
                return false;
            }

            // 3. Consultar preferências do usuário via Subscription API
            var subscriptionApiUrl = _configuration["Services:SubscriptionAPI"] ?? "http://subscription-api";
            
            // Verificar se o canal está habilitado
            var channelResponse = await _httpClient.GetAsync(
                $"{subscriptionApiUrl}/api/subscription/user/{message.UserId}/channels/{message.Channel}/enabled", 
                cancellationToken);

            if (channelResponse.IsSuccessStatusCode)
            {
                var channelContent = await channelResponse.Content.ReadAsStringAsync(cancellationToken);
                var channelResult = JsonSerializer.Deserialize<dynamic>(channelContent);
                
                // Simplificado - em produção, usar um modelo tipado
                if (channelResult?.GetProperty("enabled").GetBoolean() != true)
                {
                    _logger.LogInformation("Canal {Channel} não está habilitado para usuário {UserId}", 
                        message.Channel, message.UserId);
                    return false;
                }
            }

            // 4. Verificar se a categoria está habilitada
            if (!string.IsNullOrEmpty(message.Category))
            {
                var categoryResponse = await _httpClient.GetAsync(
                    $"{subscriptionApiUrl}/api/subscription/user/{message.UserId}/categories/{message.Category}/enabled", 
                    cancellationToken);

                if (categoryResponse.IsSuccessStatusCode)
                {
                    var categoryContent = await categoryResponse.Content.ReadAsStringAsync(cancellationToken);
                    var categoryResult = JsonSerializer.Deserialize<dynamic>(categoryContent);
                    
                    if (categoryResult?.GetProperty("enabled").GetBoolean() != true)
                    {
                        _logger.LogInformation("Categoria {Category} não está habilitada para usuário {UserId}", 
                            message.Category, message.UserId);
                        return false;
                    }
                }
            }

            // 5. Verificar horário de silêncio
            var quietHoursResponse = await _httpClient.GetAsync(
                $"{subscriptionApiUrl}/api/subscription/user/{message.UserId}/quiet-hours", 
                cancellationToken);

            if (quietHoursResponse.IsSuccessStatusCode)
            {
                var quietHoursContent = await quietHoursResponse.Content.ReadAsStringAsync(cancellationToken);
                var quietHoursResult = JsonSerializer.Deserialize<dynamic>(quietHoursContent);
                
                if (quietHoursResult?.GetProperty("inQuietHours").GetBoolean() == true)
                {
                    // Permitir notificações críticas mesmo em horário de silêncio
                    if (message.Priority != NotificationPriority.Critical)
                    {
                        _logger.LogInformation("Usuário {UserId} está em horário de silêncio, notificação {NotificationId} será adiada", 
                            message.UserId, message.Id);
                        return false;
                    }
                }
            }

            _logger.LogDebug("Notificação {NotificationId} validada com sucesso", message.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar notificação {NotificationId}", message.Id);
            return false;
        }
    }

    public async Task UpdateNotificationStatusAsync(string notificationId, string status, ProcessingResult? result = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Atualizando status da notificação {NotificationId} para {Status}", notificationId, status);

            var notificationApiUrl = _configuration["Services:NotificationAPI"] ?? "http://notification-api";
            
            var updateData = new
            {
                status = status,
                processedAt = DateTime.UtcNow,
                result = result != null ? new
                {
                    success = result.Success,
                    errorMessage = result.ErrorMessage,
                    providerId = result.ProviderId,
                    processingTime = result.ProcessingTime.TotalMilliseconds,
                    metadata = result.Metadata
                } : null
            };

            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"{notificationApiUrl}/api/notification/{notificationId}/status", 
                content, 
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao atualizar status da notificação {NotificationId}: {StatusCode}", 
                    notificationId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status da notificação {NotificationId}", notificationId);
        }
    }

    private INotificationProvider? GetProviderForChannel(string channel)
    {
        return channel.ToLower() switch
        {
            "email" => _serviceProvider.GetService<IEmailProvider>(),
            "sms" => _serviceProvider.GetService<ISmsProvider>(),
            "push" => _serviceProvider.GetService<IPushProvider>(),
            _ => null
        };
    }
} 