using Notification.API.Models;
using Notification.API.Repositories;

namespace Notification.API.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IMessageQueueService _messageQueueService;
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _configuration;

    public NotificationService(
        INotificationRepository repository,
        IMessageQueueService messageQueueService,
        ILogger<NotificationService> logger,
        IConfiguration configuration)
    {
        _repository = repository;
        _messageQueueService = messageQueueService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Criando nova notificação para usuário {UserId}", request.UserId);

            // Validar entrada
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Message))
            {
                throw new ArgumentException("UserId e Message são obrigatórios");
            }

            // Criar modelo de notificação
            var notification = new NotificationModel
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Channels = request.Channels?.Any() == true ? request.Channels : new List<string> { "email" },
                Metadata = request.Metadata,
                Status = NotificationStatus.Pending
            };

            // Salvar no banco
            var savedNotification = await _repository.CreateAsync(notification);

            // Enviar para fila de processamento
            var queueName = _configuration["ServiceBus:QueueName"] ?? "notification-queue";
            await _messageQueueService.SendMessageAsync(new { NotificationId = savedNotification.Id }, queueName);

            _logger.LogInformation("Notificação {NotificationId} criada e enviada para processamento", savedNotification.Id);

            return new NotificationResponse
            {
                Id = savedNotification.Id,
                Status = savedNotification.Status,
                Message = "Notificação criada com sucesso",
                CreatedAt = savedNotification.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar notificação para usuário {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<NotificationModel?> GetNotificationAsync(string id, string userId)
    {
        try
        {
            return await _repository.GetByIdAsync(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificação {NotificationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(string userId)
    {
        try
        {
            return await _repository.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificações do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task ProcessNotificationAsync(string notificationId)
    {
        try
        {
            _logger.LogInformation("Processando notificação {NotificationId}", notificationId);

            // Buscar notificação (precisamos do userId para a partition key)
            var pendingNotifications = await _repository.GetPendingNotificationsAsync();
            var notification = pendingNotifications.FirstOrDefault(n => n.Id == notificationId);

            if (notification == null)
            {
                _logger.LogWarning("Notificação {NotificationId} não encontrada", notificationId);
                return;
            }

            // Atualizar status para processando
            notification.Status = NotificationStatus.Processing;
            notification.Attempts++;
            await _repository.UpdateAsync(notification);

            // Simular processamento (aqui seria a integração com provedores reais)
            await SimulateNotificationProcessing(notification);

            // Atualizar status para enviado
            notification.Status = NotificationStatus.Sent;
            await _repository.UpdateAsync(notification);

            _logger.LogInformation("Notificação {NotificationId} processada com sucesso", notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar notificação {NotificationId}", notificationId);
            
            // Atualizar status para falha
            var pendingNotifications = await _repository.GetPendingNotificationsAsync();
            var notification = pendingNotifications.FirstOrDefault(n => n.Id == notificationId);
            
            if (notification != null)
            {
                notification.Status = NotificationStatus.Failed;
                notification.LastError = ex.Message;
                await _repository.UpdateAsync(notification);
            }
        }
    }

    public async Task<bool> CancelNotificationAsync(string id, string userId)
    {
        try
        {
            var notification = await _repository.GetByIdAsync(id, userId);
            if (notification == null)
            {
                return false;
            }

            if (notification.Status == NotificationStatus.Sent)
            {
                throw new InvalidOperationException("Não é possível cancelar uma notificação já enviada");
            }

            notification.Status = NotificationStatus.Cancelled;
            await _repository.UpdateAsync(notification);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar notificação {NotificationId}", id);
            throw;
        }
    }

    private async Task SimulateNotificationProcessing(NotificationModel notification)
    {
        // Simular tempo de processamento
        await Task.Delay(1000);

        _logger.LogInformation("Enviando notificação via canais: {Channels}", string.Join(", ", notification.Channels));

        foreach (var channel in notification.Channels)
        {
            switch (channel.ToLower())
            {
                case "email":
                    _logger.LogInformation("📧 Email enviado para usuário {UserId}: {Title}", notification.UserId, notification.Title);
                    break;
                case "sms":
                    _logger.LogInformation("📱 SMS enviado para usuário {UserId}: {Message}", notification.UserId, notification.Message);
                    break;
                case "push":
                    _logger.LogInformation("🔔 Push notification enviado para usuário {UserId}: {Title}", notification.UserId, notification.Title);
                    break;
                default:
                    _logger.LogWarning("Canal desconhecido: {Channel}", channel);
                    break;
            }
        }
    }
} 