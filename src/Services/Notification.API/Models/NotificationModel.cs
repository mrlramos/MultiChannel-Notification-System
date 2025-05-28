using System.Text.Json.Serialization;

namespace Notification.API.Models;

public class NotificationModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("channels")]
    public List<string> Channels { get; set; } = new();

    [JsonPropertyName("status")]
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [JsonPropertyName("attempts")]
    public int Attempts { get; set; } = 0;

    [JsonPropertyName("lastError")]
    public string? LastError { get; set; }

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => UserId;
}

public enum NotificationStatus
{
    Pending,
    Processing,
    Sent,
    Failed,
    Cancelled
}

public class NotificationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

public class NotificationResponse
{
    public string Id { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
} 