using Subscription.API.Models;
using Subscription.API.Repositories;

namespace Subscription.API.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(ISubscriptionRepository repository, ILogger<SubscriptionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SubscriptionResponse> CreateSubscriptionAsync(CreateSubscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Criando subscrição para usuário {UserId}", request.UserId);

            // Validar entrada
            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new ArgumentException("UserId é obrigatório");
            }

            // Verificar se já existe subscrição para o usuário
            var existingSubscription = await _repository.GetByUserIdAsync(request.UserId);
            if (existingSubscription != null)
            {
                throw new InvalidOperationException($"Usuário {request.UserId} já possui uma subscrição");
            }

            // Criar modelo de subscrição
            var subscription = new SubscriptionModel
            {
                UserId = request.UserId,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Preferences = request.Preferences ?? new NotificationPreferences(),
                IsActive = true
            };

            // Salvar no banco
            var savedSubscription = await _repository.CreateAsync(subscription);

            _logger.LogInformation("Subscrição {SubscriptionId} criada para usuário {UserId}", 
                savedSubscription.Id, savedSubscription.UserId);

            return MapToResponse(savedSubscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar subscrição para usuário {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<SubscriptionResponse?> GetSubscriptionByUserIdAsync(string userId)
    {
        try
        {
            var subscription = await _repository.GetByUserIdAsync(userId);
            return subscription != null ? MapToResponse(subscription) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar subscrição do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<SubscriptionResponse> UpdateSubscriptionAsync(string userId, UpdateSubscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Atualizando subscrição do usuário {UserId}", userId);

            var subscription = await _repository.GetByUserIdAsync(userId);
            if (subscription == null)
            {
                throw new InvalidOperationException($"Subscrição não encontrada para usuário {userId}");
            }

            // Atualizar campos se fornecidos
            if (!string.IsNullOrEmpty(request.Email))
                subscription.Email = request.Email;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                subscription.PhoneNumber = request.PhoneNumber;

            if (request.Preferences != null)
                subscription.Preferences = request.Preferences;

            if (request.IsActive.HasValue)
                subscription.IsActive = request.IsActive.Value;

            // Salvar alterações
            var updatedSubscription = await _repository.UpdateAsync(subscription);

            _logger.LogInformation("Subscrição {SubscriptionId} atualizada para usuário {UserId}", 
                updatedSubscription.Id, userId);

            return MapToResponse(updatedSubscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar subscrição do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteSubscriptionAsync(string userId)
    {
        try
        {
            var subscription = await _repository.GetByUserIdAsync(userId);
            if (subscription == null)
            {
                return false;
            }

            await _repository.DeleteAsync(subscription.Id, userId);
            
            _logger.LogInformation("Subscrição deletada para usuário {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar subscrição do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<List<string>> GetUserPreferredChannelsAsync(string userId)
    {
        try
        {
            var subscription = await _repository.GetByUserIdAsync(userId);
            if (subscription == null || !subscription.IsActive)
            {
                return new List<string> { "email" }; // Canal padrão
            }

            var channels = new List<string>();
            
            if (subscription.Preferences.Channels.Email)
                channels.Add("email");
            
            if (subscription.Preferences.Channels.Sms)
                channels.Add("sms");
            
            if (subscription.Preferences.Channels.Push)
                channels.Add("push");

            return channels.Any() ? channels : new List<string> { "email" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar canais preferidos do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IsChannelEnabledForUserAsync(string userId, string channel)
    {
        try
        {
            var subscription = await _repository.GetByUserIdAsync(userId);
            if (subscription == null || !subscription.IsActive)
            {
                return channel.ToLower() == "email"; // Email é padrão
            }

            return channel.ToLower() switch
            {
                "email" => subscription.Preferences.Channels.Email,
                "sms" => subscription.Preferences.Channels.Sms,
                "push" => subscription.Preferences.Channels.Push,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar canal {Channel} para usuário {UserId}", channel, userId);
            throw;
        }
    }

    public async Task<bool> IsCategoryEnabledForUserAsync(string userId, string category)
    {
        try
        {
            var subscription = await _repository.GetByUserIdAsync(userId);
            if (subscription == null || !subscription.IsActive)
            {
                return category.ToLower() is "transactional" or "security" or "system"; // Categorias padrão
            }

            return category.ToLower() switch
            {
                "marketing" => subscription.Preferences.Categories.Marketing,
                "transactional" => subscription.Preferences.Categories.Transactional,
                "security" => subscription.Preferences.Categories.Security,
                "system" => subscription.Preferences.Categories.System,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar categoria {Category} para usuário {UserId}", category, userId);
            throw;
        }
    }

    public async Task<bool> IsInQuietHoursAsync(string userId)
    {
        try
        {
            var subscription = await _repository.GetByUserIdAsync(userId);
            if (subscription?.Preferences.QuietHours?.Enabled != true)
            {
                return false;
            }

            var quietHours = subscription.Preferences.QuietHours;
            var now = TimeOnly.FromDateTime(DateTime.UtcNow); // Simplificado - em produção usar timezone do usuário

            // Verificar se está dentro do horário de silêncio
            if (quietHours.StartTime <= quietHours.EndTime)
            {
                // Mesmo dia (ex: 22:00 - 08:00 do dia seguinte)
                return now >= quietHours.StartTime || now <= quietHours.EndTime;
            }
            else
            {
                // Atravessa meia-noite (ex: 08:00 - 22:00)
                return now >= quietHours.StartTime && now <= quietHours.EndTime;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar horário de silêncio para usuário {UserId}", userId);
            return false; // Em caso de erro, não bloquear notificação
        }
    }

    private static SubscriptionResponse MapToResponse(SubscriptionModel subscription)
    {
        return new SubscriptionResponse
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            Email = subscription.Email,
            PhoneNumber = subscription.PhoneNumber,
            Preferences = subscription.Preferences,
            IsActive = subscription.IsActive,
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt
        };
    }
} 