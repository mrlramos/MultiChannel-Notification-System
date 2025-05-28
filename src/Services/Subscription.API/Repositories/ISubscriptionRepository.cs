using Subscription.API.Models;

namespace Subscription.API.Repositories;

public interface ISubscriptionRepository
{
    Task<SubscriptionModel> CreateAsync(SubscriptionModel subscription);
    Task<SubscriptionModel?> GetByUserIdAsync(string userId);
    Task<SubscriptionModel?> GetByIdAsync(string id, string userId);
    Task<SubscriptionModel> UpdateAsync(SubscriptionModel subscription);
    Task DeleteAsync(string id, string userId);
    Task<IEnumerable<SubscriptionModel>> GetActiveSubscriptionsAsync();
    Task<bool> ExistsAsync(string userId);
} 