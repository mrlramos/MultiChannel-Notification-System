using Processor.Worker.Models;

namespace Processor.Worker.Services;

public interface INotificationProcessor
{
    Task<ProcessingResult> ProcessNotificationAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    Task<bool> ValidateNotificationAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    Task UpdateNotificationStatusAsync(string notificationId, string status, ProcessingResult? result = null, CancellationToken cancellationToken = default);
} 