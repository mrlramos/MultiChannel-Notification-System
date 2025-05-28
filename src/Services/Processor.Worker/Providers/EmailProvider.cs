using Processor.Worker.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Processor.Worker.Providers;

public class EmailProvider : IEmailProvider
{
    private readonly ILogger<EmailProvider> _logger;
    private readonly HttpClient _httpClient;

    public string Channel => "email";

    public EmailProvider(ILogger<EmailProvider> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<ProcessingResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Processando email para usuário {UserId} - Notificação {NotificationId}", 
                message.UserId, message.Id);

            // Extrair email dos metadados
            var email = ExtractEmailFromMetadata(message);
            if (string.IsNullOrEmpty(email))
            {
                return new ProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Email não encontrado nos metadados da mensagem",
                    ProcessingTime = stopwatch.Elapsed
                };
            }

            return await SendEmailAsync(email, message.Title, message.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar email para usuário {UserId}", message.UserId);
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<ProcessingResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Enviando email para {Email} com assunto '{Subject}'", to, subject);

            // Simular envio de email (em produção, integrar com SendGrid, AWS SES, etc.)
            await SimulateEmailSending(to, subject, body, cancellationToken);

            var result = new ProcessingResult
            {
                Success = true,
                ProviderId = $"email_{Guid.NewGuid():N}",
                ProcessingTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    ["recipient"] = to,
                    ["subject"] = subject,
                    ["provider"] = "EmailProvider",
                    ["sentAt"] = DateTime.UtcNow
                }
            };

            _logger.LogInformation("Email enviado com sucesso para {Email} - Provider ID: {ProviderId}", 
                to, result.ProviderId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {Email}", to);
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
            // Simular health check do provedor de email
            await Task.Delay(100, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string? ExtractEmailFromMetadata(NotificationMessage message)
    {
        if (message.Metadata.TryGetValue("email", out var emailObj))
        {
            return emailObj?.ToString();
        }

        if (message.Metadata.TryGetValue("recipient", out var recipientObj))
        {
            return recipientObj?.ToString();
        }

        return null;
    }

    private async Task SimulateEmailSending(string to, string subject, string body, CancellationToken cancellationToken)
    {
        // Simular latência de envio
        var delay = Random.Shared.Next(500, 2000);
        await Task.Delay(delay, cancellationToken);

        // Simular falha ocasional (5% de chance)
        if (Random.Shared.Next(1, 101) <= 5)
        {
            throw new InvalidOperationException("Falha simulada no provedor de email");
        }

        // Log do email "enviado"
        _logger.LogDebug("📧 EMAIL SIMULADO ENVIADO:\nPara: {To}\nAssunto: {Subject}\nCorpo: {Body}", 
            to, subject, body);
    }
} 