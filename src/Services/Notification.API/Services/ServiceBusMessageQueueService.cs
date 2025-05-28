using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Notification.API.Services;

public class ServiceBusMessageQueueService : IMessageQueueService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<ServiceBusMessageQueueService> _logger;

    public ServiceBusMessageQueueService(ServiceBusClient serviceBusClient, ILogger<ServiceBusMessageQueueService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        try
        {
            _logger.LogInformation("Enviando mensagem para fila {QueueName}", queueName);

            var sender = _serviceBusClient.CreateSender(queueName);
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody);

            await sender.SendMessageAsync(serviceBusMessage);
            
            _logger.LogInformation("Mensagem enviada com sucesso para fila {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem para fila {QueueName}", queueName);
            
            // Para desenvolvimento local, apenas log o erro sem falhar
            if (ex.Message.Contains("localhost") || ex.Message.Contains("fake"))
            {
                _logger.LogWarning("Simulando envio de mensagem para desenvolvimento local");
                return;
            }
            
            throw;
        }
    }

    public async Task<T?> ReceiveMessageAsync<T>(string queueName)
    {
        try
        {
            var receiver = _serviceBusClient.CreateReceiver(queueName);
            var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));

            if (message != null)
            {
                var messageBody = message.Body.ToString();
                var deserializedMessage = JsonSerializer.Deserialize<T>(messageBody);
                
                // Marcar mensagem como processada
                await receiver.CompleteMessageAsync(message);
                
                return deserializedMessage;
            }

            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao receber mensagem da fila {QueueName}", queueName);
            
            // Para desenvolvimento local, retornar null
            if (ex.Message.Contains("localhost") || ex.Message.Contains("fake"))
            {
                return default(T);
            }
            
            throw;
        }
    }
} 