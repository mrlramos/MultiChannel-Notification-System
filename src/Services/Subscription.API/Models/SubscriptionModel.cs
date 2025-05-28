using System.Text.Json.Serialization;

namespace Subscription.API.Models;

public class SubscriptionModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("preferences")]
    public NotificationPreferences Preferences { get; set; } = new();

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => UserId;
}

public class NotificationPreferences
{
    [JsonPropertyName("channels")]
    public ChannelPreferences Channels { get; set; } = new();

    [JsonPropertyName("categories")]
    public CategoryPreferences Categories { get; set; } = new();

    [JsonPropertyName("quietHours")]
    public QuietHours? QuietHours { get; set; }

    [JsonPropertyName("frequency")]
    public NotificationFrequency Frequency { get; set; } = NotificationFrequency.Immediate;
}

public class ChannelPreferences
{
    [JsonPropertyName("email")]
    public bool Email { get; set; } = true;

    [JsonPropertyName("sms")]
    public bool Sms { get; set; } = false;

    [JsonPropertyName("push")]
    public bool Push { get; set; } = true;
}

public class CategoryPreferences
{
    [JsonPropertyName("marketing")]
    public bool Marketing { get; set; } = false;

    [JsonPropertyName("transactional")]
    public bool Transactional { get; set; } = true;

    [JsonPropertyName("security")]
    public bool Security { get; set; } = true;

    [JsonPropertyName("system")]
    public bool System { get; set; } = true;
}

public class QuietHours
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("startTime")]
    public TimeOnly StartTime { get; set; } = new(22, 0); // 22:00

    [JsonPropertyName("endTime")]
    public TimeOnly EndTime { get; set; } = new(8, 0); // 08:00

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = "UTC";
}

public enum NotificationFrequency
{
    Immediate,
    Hourly,
    Daily,
    Weekly
}

// DTOs para requests/responses
public class CreateSubscriptionRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public NotificationPreferences? Preferences { get; set; }
}

public class UpdateSubscriptionRequest
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public NotificationPreferences? Preferences { get; set; }
    public bool? IsActive { get; set; }
}

public class SubscriptionResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public NotificationPreferences Preferences { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 