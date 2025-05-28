using Notification.API.Models;
using System.Collections.Concurrent;

namespace Notification.API.Repositories;

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly ConcurrentDictionary<string, NotificationModel> _notifications = new();

    public async Task<NotificationModel> CreateAsync(NotificationModel notification)
    {
        notification.Id = Guid.NewGuid().ToString();
        notification.CreatedAt = DateTime.UtcNow;
        notification.Status = NotificationStatus.Pending;

        _notifications.TryAdd(notification.Id, notification);
        
        await Task.Delay(10); // Simular operação assíncrona
        return notification;
    }

    public async Task<NotificationModel?> GetByIdAsync(string id, string userId)
    {
        await Task.Delay(10); // Simular operação assíncrona
        _notifications.TryGetValue(id, out var notification);
        
        if (notification?.UserId == userId)
            return notification;
            
        return null;
    }

    public async Task<IEnumerable<NotificationModel>> GetByUserIdAsync(string userId)
    {
        await Task.Delay(10); // Simular operação assíncrona
        return _notifications.Values.Where(n => n.UserId == userId).ToList();
    }

    public async Task<NotificationModel> UpdateAsync(NotificationModel notification)
    {
        notification.UpdatedAt = DateTime.UtcNow;
        _notifications.TryUpdate(notification.Id, notification, _notifications[notification.Id]);
        
        await Task.Delay(10); // Simular operação assíncrona
        return notification;
    }

    public async Task DeleteAsync(string id, string userId)
    {
        await Task.Delay(10); // Simular operação assíncrona
        
        if (_notifications.TryGetValue(id, out var notification) && notification.UserId == userId)
        {
            _notifications.TryRemove(id, out _);
        }
    }

    public async Task<IEnumerable<NotificationModel>> GetPendingNotificationsAsync()
    {
        await Task.Delay(10); // Simular operação assíncrona
        return _notifications.Values.Where(n => n.Status == NotificationStatus.Pending).ToList();
    }
} 