using Notification.API.Models;

namespace Notification.API.Services;

public interface INotificationService
{
    Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request);
    Task<NotificationModel?> GetNotificationAsync(string id, string userId);
    Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(string userId);
    Task ProcessNotificationAsync(string notificationId);
    Task<bool> CancelNotificationAsync(string id, string userId);
}

public interface IMessageQueueService
{
    Task SendMessageAsync<T>(T message, string queueName);
    Task<T?> ReceiveMessageAsync<T>(string queueName);
} 