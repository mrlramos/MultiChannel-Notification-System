using Microsoft.AspNetCore.Mvc;
using Notification.API.Models;
using Notification.API.Services;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Recebendo requisição para criar notificação para usuário {UserId}", request.UserId);

            var response = await _notificationService.CreateNotificationAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos na requisição");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar notificação");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(string id, [FromQuery] string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { error = "UserId é obrigatório" });
            }

            var notification = await _notificationService.GetNotificationAsync(id, userId);
            
            if (notification == null)
            {
                return NotFound(new { message = "Notificação não encontrada" });
            }

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificação {NotificationId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(string userId)
    {
        try
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificações do usuário {UserId}", userId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessNotification(string id)
    {
        try
        {
            await _notificationService.ProcessNotificationAsync(id);
            return Ok(new { message = "Notificação processada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar notificação {NotificationId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelNotification(string id, [FromQuery] string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { error = "UserId é obrigatório" });
            }

            var cancelled = await _notificationService.CancelNotificationAsync(id, userId);
            
            if (!cancelled)
            {
                return NotFound(new { message = "Notificação não encontrada" });
            }

            return Ok(new { message = "Notificação cancelada com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar notificação {NotificationId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "notification-api", timestamp = DateTime.UtcNow });
    }
} 