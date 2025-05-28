using Processor.Worker.Models;

namespace Processor.Worker.Providers;

public interface INotificationProvider
{
    string Channel { get; }
    Task<ProcessingResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}

public interface IEmailProvider : INotificationProvider
{
    Task<ProcessingResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

public interface ISmsProvider : INotificationProvider
{
    Task<ProcessingResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}

public interface IPushProvider : INotificationProvider
{
    Task<ProcessingResult> SendPushAsync(string deviceToken, string title, string body, Dictionary<string, object>? data = null, CancellationToken cancellationToken = default);
} 