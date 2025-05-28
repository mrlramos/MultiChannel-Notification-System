using Microsoft.AspNetCore.Mvc;
using Subscription.API.Models;
using Subscription.API.Services;

namespace Subscription.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(ISubscriptionService subscriptionService, ILogger<SubscriptionController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Recebendo requisição para criar subscrição para usuário {UserId}", request.UserId);

            var response = await _subscriptionService.CreateSubscriptionAsync(request);
            return CreatedAtAction(nameof(GetSubscription), new { userId = response.UserId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos na requisição");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida");
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar subscrição");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetSubscription(string userId)
    {
        try
        {
            var subscription = await _subscriptionService.GetSubscriptionByUserIdAsync(userId);
            
            if (subscription == null)
            {
                return NotFound(new { message = "Subscrição não encontrada" });
            }

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar subscrição do usuário {UserId}", userId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPut("user/{userId}")]
    public async Task<IActionResult> UpdateSubscription(string userId, [FromBody] UpdateSubscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Recebendo requisição para atualizar subscrição do usuário {UserId}", userId);

            var response = await _subscriptionService.UpdateSubscriptionAsync(userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar subscrição do usuário {UserId}", userId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete("user/{userId}")]
    public async Task<IActionResult> DeleteSubscription(string userId)
    {
        try
        {
            var deleted = await _subscriptionService.DeleteSubscriptionAsync(userId);
            
            if (!deleted)
            {
                return NotFound(new { message = "Subscrição não encontrada" });
            }

            return Ok(new { message = "Subscrição deletada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar subscrição do usuário {UserId}", userId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("user/{userId}/channels")]
    public async Task<IActionResult> GetUserPreferredChannels(string userId)
    {
        try
        {
            var channels = await _subscriptionService.GetUserPreferredChannelsAsync(userId);
            return Ok(new { userId, channels });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar canais preferidos do usuário {UserId}", userId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("user/{userId}/channels/{channel}/enabled")]
    public async Task<IActionResult> IsChannelEnabled(string userId, string channel)
    {
        try
        {
            var enabled = await _subscriptionService.IsChannelEnabledForUserAsync(userId, channel);
            return Ok(new { userId, channel, enabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar canal {Channel} para usuário {UserId}", channel, userId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("user/{userId}/categories/{category}/enabled")]
    public async Task<IActionResult> IsCategoryEnabled(string userId, string category)
    {
        try
        {
            var enabled = await _subscriptionService.IsCategoryEnabledForUserAsync(userId, category);
            return Ok(new { userId, category, enabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar categoria {Category} para usuário {UserId}", category, userId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("user/{userId}/quiet-hours")]
    public async Task<IActionResult> IsInQuietHours(string userId)
    {
        try
        {
            var inQuietHours = await _subscriptionService.IsInQuietHoursAsync(userId);
            return Ok(new { userId, inQuietHours, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar horário de silêncio para usuário {UserId}", userId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "subscription-api", timestamp = DateTime.UtcNow });
    }
} 