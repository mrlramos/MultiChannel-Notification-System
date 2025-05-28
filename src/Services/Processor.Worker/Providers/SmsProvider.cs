using Processor.Worker.Models;
using System.Diagnostics;

namespace Processor.Worker.Providers;

public class SmsProvider : ISmsProvider
{
    private readonly ILogger<SmsProvider> _logger;
    private readonly HttpClient _httpClient;

    public string Channel => "sms";

    public SmsProvider(ILogger<SmsProvider> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<ProcessingResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Processando SMS para usuário {UserId} - Notificação {NotificationId}", 
                message.UserId, message.Id);

            // Extrair telefone dos metadados
            var phoneNumber = ExtractPhoneFromMetadata(message);
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return new ProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Número de telefone não encontrado nos metadados da mensagem",
                    ProcessingTime = stopwatch.Elapsed
                };
            }

            // Combinar título e conteúdo para SMS
            var smsContent = !string.IsNullOrEmpty(message.Title) 
                ? $"{message.Title}: {message.Content}" 
                : message.Content;

            return await SendSmsAsync(phoneNumber, smsContent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar SMS para usuário {UserId}", message.UserId);
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<ProcessingResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Enviando SMS para {PhoneNumber}", MaskPhoneNumber(phoneNumber));

            // Validar número de telefone
            if (!IsValidPhoneNumber(phoneNumber))
            {
                return new ProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Número de telefone inválido",
                    ProcessingTime = stopwatch.Elapsed
                };
            }

            // Validar tamanho da mensagem (limite típico de SMS)
            if (message.Length > 160)
            {
                _logger.LogWarning("Mensagem SMS muito longa ({Length} caracteres), será truncada", message.Length);
                message = message[..157] + "...";
            }

            // Simular envio de SMS (em produção, integrar com Twilio, AWS SNS, etc.)
            await SimulateSmsSending(phoneNumber, message, cancellationToken);

            var result = new ProcessingResult
            {
                Success = true,
                ProviderId = $"sms_{Guid.NewGuid():N}",
                ProcessingTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    ["recipient"] = MaskPhoneNumber(phoneNumber),
                    ["messageLength"] = message.Length,
                    ["provider"] = "SmsProvider",
                    ["sentAt"] = DateTime.UtcNow
                }
            };

            _logger.LogInformation("SMS enviado com sucesso para {PhoneNumber} - Provider ID: {ProviderId}", 
                MaskPhoneNumber(phoneNumber), result.ProviderId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar SMS para {PhoneNumber}", MaskPhoneNumber(phoneNumber));
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
            // Simular health check do provedor de SMS
            await Task.Delay(150, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string? ExtractPhoneFromMetadata(NotificationMessage message)
    {
        if (message.Metadata.TryGetValue("phoneNumber", out var phoneObj))
        {
            return phoneObj?.ToString();
        }

        if (message.Metadata.TryGetValue("phone", out var phoneObj2))
        {
            return phoneObj2?.ToString();
        }

        return null;
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Validação básica de número de telefone
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove caracteres não numéricos
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        // Deve ter entre 10 e 15 dígitos
        return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return "****";

        return phoneNumber[..2] + "****" + phoneNumber[^2..];
    }

    private async Task SimulateSmsSending(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        // Simular latência de envio
        var delay = Random.Shared.Next(300, 1500);
        await Task.Delay(delay, cancellationToken);

        // Simular falha ocasional (3% de chance)
        if (Random.Shared.Next(1, 101) <= 3)
        {
            throw new InvalidOperationException("Falha simulada no provedor de SMS");
        }

        // Log do SMS "enviado"
        _logger.LogDebug("📱 SMS SIMULADO ENVIADO:\nPara: {PhoneNumber}\nMensagem: {Message}", 
            MaskPhoneNumber(phoneNumber), message);
    }
} 