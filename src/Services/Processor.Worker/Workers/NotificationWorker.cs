using Azure.Messaging.ServiceBus;
using Processor.Worker.Models;
using Processor.Worker.Services;
using System.Text.Json;
using Polly;
using Polly.Extensions.Http;

namespace Processor.Worker.Workers;

public class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private ServiceBusClient? _serviceBusClient;
    private ServiceBusProcessor? _processor;

    public NotificationWorker(
        ILogger<NotificationWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 Iniciando Notification Worker...");

        try
        {
            // Configurar Service Bus
            var connectionString = _configuration.GetConnectionString("ServiceBus");
            var queueName = _configuration["ServiceBus:QueueName"] ?? "notifications";

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("⚠️ Connection string do Service Bus não configurada, usando modo simulação");
                await base.StartAsync(cancellationToken);
                return;
            }

            _serviceBusClient = new ServiceBusClient(connectionString);
            
            var processorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = _configuration.GetValue<int>("ServiceBus:MaxConcurrentCalls", 5),
                AutoCompleteMessages = false,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
            };

            _processor = _serviceBusClient.CreateProcessor(queueName, processorOptions);
            
            // Configurar handlers
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            // Iniciar processamento
            await _processor.StartProcessingAsync(cancellationToken);
            
            _logger.LogInformation("✅ Notification Worker iniciado com sucesso. Aguardando mensagens...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao iniciar Notification Worker");
            throw;
        }

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Parando Notification Worker...");

        try
        {
            if (_processor != null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }

            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync();
            }

            _logger.LogInformation("✅ Notification Worker parado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao parar Notification Worker");
        }

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Se não há Service Bus configurado, simular processamento
        if (_processor == null)
        {
            await SimulateProcessingAsync(stoppingToken);
            return;
        }

        // Aguardar até ser cancelado
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            _logger.LogDebug("💓 Worker ativo, processando mensagens...");
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var messageId = args.Message.MessageId;
        var correlationId = args.Message.CorrelationId;

        try
        {
            _logger.LogInformation("📨 Recebida mensagem {MessageId} (Correlation: {CorrelationId})", 
                messageId, correlationId);

            // Deserializar mensagem
            var messageBody = args.Message.Body.ToString();
            var notificationMessage = JsonSerializer.Deserialize<NotificationMessage>(messageBody);

            if (notificationMessage == null)
            {
                _logger.LogError("❌ Falha ao deserializar mensagem {MessageId}", messageId);
                await args.DeadLetterMessageAsync(args.Message, "InvalidMessage", "Falha na deserialização", args.CancellationToken);
                return;
            }

            // Processar com retry policy
            var retryPolicy = GetRetryPolicy();
            var result = await retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();
                return await processor.ProcessNotificationAsync(notificationMessage, args.CancellationToken);
            });

            if (result.Success)
            {
                _logger.LogInformation("✅ Mensagem {MessageId} processada com sucesso", messageId);
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            else
            {
                // Verificar tentativas
                var deliveryCount = args.Message.DeliveryCount;
                var maxAttempts = notificationMessage.MaxAttempts;

                if (deliveryCount >= maxAttempts)
                {
                    _logger.LogError("💀 Mensagem {MessageId} excedeu máximo de tentativas ({DeliveryCount}/{MaxAttempts}), enviando para DLQ", 
                        messageId, deliveryCount, maxAttempts);
                    
                    await args.DeadLetterMessageAsync(args.Message, "MaxAttemptsExceeded", 
                        $"Falha após {deliveryCount} tentativas: {result.ErrorMessage}", 
                        args.CancellationToken);
                }
                else
                {
                    _logger.LogWarning("⚠️ Falha no processamento da mensagem {MessageId} (tentativa {DeliveryCount}/{MaxAttempts}): {Error}", 
                        messageId, deliveryCount, maxAttempts, result.ErrorMessage);
                    
                    // Abandonar para retry automático
                    await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar mensagem {MessageId}", messageId);
            
            try
            {
                await args.DeadLetterMessageAsync(args.Message, "ProcessingError", ex.Message, args.CancellationToken);
            }
            catch (Exception dlqEx)
            {
                _logger.LogError(dlqEx, "❌ Erro ao enviar mensagem {MessageId} para DLQ", messageId);
            }
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "❌ Erro no processamento do Service Bus: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }

    private async Task SimulateProcessingAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🎭 Modo simulação ativado - gerando notificações de teste...");

        var random = new Random();
        var userIds = new[] { "user1", "user2", "user3", "user4", "user5" };
        var channels = new[] { "email", "sms", "push" };
        var categories = new[] { "marketing", "transactional", "security", "system" };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Gerar notificação simulada a cada 30-60 segundos
                var delay = TimeSpan.FromSeconds(random.Next(30, 61));
                await Task.Delay(delay, stoppingToken);

                var simulatedMessage = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userIds[random.Next(userIds.Length)],
                    Title = "Notificação de Teste",
                    Content = $"Esta é uma notificação simulada gerada em {DateTime.Now:HH:mm:ss}",
                    Channel = channels[random.Next(channels.Length)],
                    Category = categories[random.Next(categories.Length)],
                    Priority = (NotificationPriority)random.Next(0, 4),
                    Metadata = new Dictionary<string, object>
                    {
                        ["email"] = "usuario@exemplo.com",
                        ["phoneNumber"] = "+5511999999999",
                        ["deviceToken"] = "fake_device_token_" + Guid.NewGuid().ToString("N")[..16],
                        ["simulated"] = true
                    }
                };

                _logger.LogInformation("🎭 Processando notificação simulada {NotificationId}", simulatedMessage.Id);

                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();
                var result = await processor.ProcessNotificationAsync(simulatedMessage, stoppingToken);

                var status = result.Success ? "✅ Sucesso" : "❌ Falha";
                _logger.LogInformation("{Status} - Notificação simulada {NotificationId}: {Message}", 
                    status, simulatedMessage.Id, result.ErrorMessage ?? "Processada com sucesso");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na simulação de processamento");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("🎭 Modo simulação finalizado");
    }

    private static IAsyncPolicy GetRetryPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<InvalidOperationException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential backoff
    }
} 