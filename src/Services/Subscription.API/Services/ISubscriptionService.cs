using Subscription.API.Models;

namespace Subscription.API.Services;

public interface ISubscriptionService
{
    Task<SubscriptionResponse> CreateSubscriptionAsync(CreateSubscriptionRequest request);
    Task<SubscriptionResponse?> GetSubscriptionByUserIdAsync(string userId);
    Task<SubscriptionResponse> UpdateSubscriptionAsync(string userId, UpdateSubscriptionRequest request);
    Task<bool> DeleteSubscriptionAsync(string userId);
    Task<List<string>> GetUserPreferredChannelsAsync(string userId);
    Task<bool> IsChannelEnabledForUserAsync(string userId, string channel);
    Task<bool> IsCategoryEnabledForUserAsync(string userId, string category);
    Task<bool> IsInQuietHoursAsync(string userId);
} 