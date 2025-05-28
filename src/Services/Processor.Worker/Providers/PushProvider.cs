using Processor.Worker.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Processor.Worker.Providers;

public class PushProvider : IPushProvider
{
    private readonly ILogger<PushProvider> _logger;
    private readonly HttpClient _httpClient;

    public string Channel => "push";

    public PushProvider(ILogger<PushProvider> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<ProcessingResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Processando Push para usuário {UserId} - Notificação {NotificationId}", 
                message.UserId, message.Id);

            // Extrair device token dos metadados
            var deviceToken = ExtractDeviceTokenFromMetadata(message);
            if (string.IsNullOrEmpty(deviceToken))
            {
                return new ProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Device token não encontrado nos metadados da mensagem",
                    ProcessingTime = stopwatch.Elapsed
                };
            }

            // Extrair dados adicionais dos metadados
            var additionalData = ExtractAdditionalData(message);

            return await SendPushAsync(deviceToken, message.Title, message.Content, additionalData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar Push para usuário {UserId}", message.UserId);
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<ProcessingResult> SendPushAsync(string deviceToken, string title, string body, Dictionary<string, object>? data = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Enviando Push notification para device {DeviceToken}", MaskDeviceToken(deviceToken));

            // Validar device token
            if (!IsValidDeviceToken(deviceToken))
            {
                return new ProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Device token inválido",
                    ProcessingTime = stopwatch.Elapsed
                };
            }

            // Validar tamanho do título e corpo
            if (title.Length > 100)
            {
                _logger.LogWarning("Título da push notification muito longo ({Length} caracteres), será truncado", title.Length);
                title = title[..97] + "...";
            }

            if (body.Length > 200)
            {
                _logger.LogWarning("Corpo da push notification muito longo ({Length} caracteres), será truncado", body.Length);
                body = body[..197] + "...";
            }

            // Simular envio de push (em produção, integrar com Firebase FCM, Apple APNS, etc.)
            await SimulatePushSending(deviceToken, title, body, data, cancellationToken);

            var result = new ProcessingResult
            {
                Success = true,
                ProviderId = $"push_{Guid.NewGuid():N}",
                ProcessingTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    ["deviceToken"] = MaskDeviceToken(deviceToken),
                    ["title"] = title,
                    ["titleLength"] = title.Length,
                    ["bodyLength"] = body.Length,
                    ["hasAdditionalData"] = data?.Any() == true,
                    ["provider"] = "PushProvider",
                    ["sentAt"] = DateTime.UtcNow
                }
            };

            _logger.LogInformation("Push notification enviada com sucesso para device {DeviceToken} - Provider ID: {ProviderId}", 
                MaskDeviceToken(deviceToken), result.ProviderId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar Push notification para device {DeviceToken}", MaskDeviceToken(deviceToken));
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Simular health check do provedor de push
            await Task.Delay(120, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string? ExtractDeviceTokenFromMetadata(NotificationMessage message)
    {
        if (message.Metadata.TryGetValue("deviceToken", out var tokenObj))
        {
            return tokenObj?.ToString();
        }

        if (message.Metadata.TryGetValue("pushToken", out var pushTokenObj))
        {
            return pushTokenObj?.ToString();
        }

        if (message.Metadata.TryGetValue("fcmToken", out var fcmTokenObj))
        {
            return fcmTokenObj?.ToString();
        }

        return null;
    }

    private Dictionary<string, object>? ExtractAdditionalData(NotificationMessage message)
    {
        var data = new Dictionary<string, object>();

        // Adicionar metadados relevantes
        if (message.Metadata.TryGetValue("data", out var dataObj) && dataObj is Dictionary<string, object> additionalData)
        {
            foreach (var kvp in additionalData)
            {
                data[kvp.Key] = kvp.Value;
            }
        }

        // Adicionar informações da notificação
        data["notificationId"] = message.Id;
        data["category"] = message.Category;
        data["priority"] = message.Priority.ToString();
        data["createdAt"] = message.CreatedAt.ToString("O");

        return data.Any() ? data : null;
    }

    private static bool IsValidDeviceToken(string deviceToken)
    {
        // Validação básica de device token
        if (string.IsNullOrWhiteSpace(deviceToken))
            return false;

        // Device tokens geralmente têm entre 64 e 200 caracteres
        return deviceToken.Length >= 64 && deviceToken.Length <= 200;
    }

    private static string MaskDeviceToken(string deviceToken)
    {
        if (string.IsNullOrEmpty(deviceToken) || deviceToken.Length < 8)
            return "****";

        return deviceToken[..4] + "****" + deviceToken[^4..];
    }

    private async Task SimulatePushSending(string deviceToken, string title, string body, Dictionary<string, object>? data, CancellationToken cancellationToken)
    {
        // Simular latência de envio
        var delay = Random.Shared.Next(200, 1000);
        await Task.Delay(delay, cancellationToken);

        // Simular falha ocasional (2% de chance)
        if (Random.Shared.Next(1, 101) <= 2)
        {
            throw new InvalidOperationException("Falha simulada no provedor de push");
        }

        // Log da push notification "enviada"
        var dataJson = data != null ? JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }) : "null";
        
        _logger.LogDebug("🔔 PUSH NOTIFICATION SIMULADA ENVIADA:\nDevice: {DeviceToken}\nTítulo: {Title}\nCorpo: {Body}\nDados: {Data}", 
            MaskDeviceToken(deviceToken), title, body, dataJson);
    }
} 