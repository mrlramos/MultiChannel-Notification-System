using Microsoft.Azure.Cosmos;
using Notification.API.Models;

namespace Notification.API.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly Container _container;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(CosmosClient cosmosClient, IConfiguration configuration, ILogger<NotificationRepository> logger)
    {
        var databaseName = configuration["CosmosDB:DatabaseName"] ?? "NotificationSystem";
        var containerName = configuration["CosmosDB:ContainerName"] ?? "Notifications";
        
        _container = cosmosClient.GetContainer(databaseName, containerName);
        _logger = logger;
    }

    public async Task<NotificationModel> CreateAsync(NotificationModel notification)
    {
        try
        {
            _logger.LogInformation("Criando notificação {NotificationId} para usuário {UserId}", 
                notification.Id, notification.UserId);

            var response = await _container.CreateItemAsync(notification, new PartitionKey(notification.PartitionKey));
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar notificação {NotificationId}", notification.Id);
            throw;
        }
    }

    public async Task<NotificationModel?> GetByIdAsync(string id, string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<NotificationModel>(id, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificação {NotificationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationModel>> GetByUserIdAsync(string userId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC")
                .WithParameter("@userId", userId);

            var iterator = _container.GetItemQueryIterator<NotificationModel>(query);
            var results = new List<NotificationModel>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificações do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<NotificationModel> UpdateAsync(NotificationModel notification)
    {
        try
        {
            notification.UpdatedAt = DateTime.UtcNow;
            var response = await _container.ReplaceItemAsync(notification, notification.Id, new PartitionKey(notification.PartitionKey));
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar notificação {NotificationId}", notification.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id, string userId)
    {
        try
        {
            await _container.DeleteItemAsync<NotificationModel>(id, new PartitionKey(userId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar notificação {NotificationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationModel>> GetPendingNotificationsAsync()
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.status = @status")
                .WithParameter("@status", NotificationStatus.Pending.ToString());

            var iterator = _container.GetItemQueryIterator<NotificationModel>(query);
            var results = new List<NotificationModel>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificações pendentes");
            throw;
        }
    }
} 