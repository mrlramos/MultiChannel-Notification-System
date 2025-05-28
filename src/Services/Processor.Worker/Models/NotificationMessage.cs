using System.Text.Json.Serialization;

namespace Processor.Worker.Models;

public class NotificationMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    [JsonPropertyName("scheduledFor")]
    public DateTime? ScheduledFor { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("attempts")]
    public int Attempts { get; set; } = 0;

    [JsonPropertyName("maxAttempts")]
    public int MaxAttempts { get; set; } = 3;
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public class ProcessingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProviderId { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
} 