using Microsoft.Azure.Cosmos;
using Subscription.API.Models;

namespace Subscription.API.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly Container _container;
    private readonly ILogger<SubscriptionRepository> _logger;

    public SubscriptionRepository(CosmosClient cosmosClient, IConfiguration configuration, ILogger<SubscriptionRepository> logger)
    {
        var databaseName = configuration["CosmosDB:DatabaseName"] ?? "NotificationSystem";
        var containerName = configuration["CosmosDB:ContainerName"] ?? "Subscriptions";
        
        _container = cosmosClient.GetContainer(databaseName, containerName);
        _logger = logger;
    }

    public async Task<SubscriptionModel> CreateAsync(SubscriptionModel subscription)
    {
        try
        {
            _logger.LogInformation("Criando subscrição para usuário {UserId}", subscription.UserId);

            var response = await _container.CreateItemAsync(subscription, new PartitionKey(subscription.PartitionKey));
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar subscrição para usuário {UserId}", subscription.UserId);
            throw;
        }
    }

    public async Task<SubscriptionModel?> GetByUserIdAsync(string userId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                .WithParameter("@userId", userId);

            var iterator = _container.GetItemQueryIterator<SubscriptionModel>(query);
            
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar subscrição do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<SubscriptionModel?> GetByIdAsync(string id, string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<SubscriptionModel>(id, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar subscrição {SubscriptionId}", id);
            throw;
        }
    }

    public async Task<SubscriptionModel> UpdateAsync(SubscriptionModel subscription)
    {
        try
        {
            subscription.UpdatedAt = DateTime.UtcNow;
            var response = await _container.ReplaceItemAsync(subscription, subscription.Id, new PartitionKey(subscription.PartitionKey));
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar subscrição {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id, string userId)
    {
        try
        {
            await _container.DeleteItemAsync<SubscriptionModel>(id, new PartitionKey(userId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar subscrição {SubscriptionId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<SubscriptionModel>> GetActiveSubscriptionsAsync()
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.isActive = @isActive")
                .WithParameter("@isActive", true);

            var iterator = _container.GetItemQueryIterator<SubscriptionModel>(query);
            var results = new List<SubscriptionModel>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar subscrições ativas");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        try
        {
            var subscription = await GetByUserIdAsync(userId);
            return subscription != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência de subscrição para usuário {UserId}", userId);
            throw;
        }
    }
} 