using System.Collections.Concurrent;
using System.Text.Json;

namespace Notification.API.Services;

public class InMemoryMessageQueueService : IMessageQueueService
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly ILogger<InMemoryMessageQueueService> _logger;

    public InMemoryMessageQueueService(ILogger<InMemoryMessageQueueService> logger)
    {
        _logger = logger;
    }

    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            _queue.Enqueue(json);
            
            _logger.LogInformation("Mensagem enviada para fila {QueueName}: {Message}", queueName, json);
            
            await Task.Delay(10); // Simular operação assíncrona
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem para fila {QueueName}", queueName);
            throw;
        }
    }

    public async Task<T?> ReceiveMessageAsync<T>(string queueName)
    {
        try
        {
            if (_queue.TryDequeue(out var json))
            {
                var message = JsonSerializer.Deserialize<T>(json);
                _logger.LogInformation("Mensagem recebida da fila {QueueName}: {Message}", queueName, json);
                
                await Task.Delay(10); // Simular operação assíncrona
                return message;
            }
            
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao receber mensagem da fila {QueueName}", queueName);
            throw;
        }
    }
} 