using Subscription.API.Models;
using System.Collections.Concurrent;

namespace Subscription.API.Repositories;

public class InMemorySubscriptionRepository : ISubscriptionRepository
{
    private readonly ConcurrentDictionary<string, SubscriptionModel> _subscriptions = new();

    public async Task<SubscriptionModel> CreateAsync(SubscriptionModel subscription)
    {
        subscription.Id = Guid.NewGuid().ToString();
        subscription.CreatedAt = DateTime.UtcNow;
        subscription.IsActive = true;

        _subscriptions.TryAdd(subscription.UserId, subscription);
        
        await Task.Delay(10); // Simular operação assíncrona
        return subscription;
    }

    public async Task<SubscriptionModel?> GetByUserIdAsync(string userId)
    {
        await Task.Delay(10); // Simular operação assíncrona
        _subscriptions.TryGetValue(userId, out var subscription);
        return subscription;
    }

    public async Task<SubscriptionModel?> GetByIdAsync(string id, string userId)
    {
        await Task.Delay(10); // Simular operação assíncrona
        _subscriptions.TryGetValue(userId, out var subscription);
        
        if (subscription?.Id == id)
            return subscription;
            
        return null;
    }

    public async Task<SubscriptionModel> UpdateAsync(SubscriptionModel subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        _subscriptions.TryUpdate(subscription.UserId, subscription, _subscriptions[subscription.UserId]);
        
        await Task.Delay(10); // Simular operação assíncrona
        return subscription;
    }

    public async Task DeleteAsync(string id, string userId)
    {
        await Task.Delay(10); // Simular operação assíncrona
        
        if (_subscriptions.TryGetValue(userId, out var subscription) && subscription.Id == id)
        {
            _subscriptions.TryRemove(userId, out _);
        }
    }

    public async Task<IEnumerable<SubscriptionModel>> GetActiveSubscriptionsAsync()
    {
        await Task.Delay(10); // Simular operação assíncrona
        return _subscriptions.Values.Where(s => s.IsActive).ToList();
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        await Task.Delay(10); // Simular operação assíncrona
        return _subscriptions.ContainsKey(userId);
    }
} 