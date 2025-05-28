using Notification.API.Models;

namespace Notification.API.Repositories;

public interface INotificationRepository
{
    Task<NotificationModel> CreateAsync(NotificationModel notification);
    Task<NotificationModel?> GetByIdAsync(string id, string userId);
    Task<IEnumerable<NotificationModel>> GetByUserIdAsync(string userId);
    Task<NotificationModel> UpdateAsync(NotificationModel notification);
    Task DeleteAsync(string id, string userId);
    Task<IEnumerable<NotificationModel>> GetPendingNotificationsAsync();
} 