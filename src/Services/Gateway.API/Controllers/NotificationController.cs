using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(IHttpClientFactory httpClientFactory, ILogger<NotificationController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Gateway: Criando notificação para {UserId}", request.UserId);

            var httpClient = _httpClientFactory.CreateClient("NotificationAPI");
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("api/notification", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonSerializer.Deserialize<object>(result));
            }

            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao criar notificação");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(string id)
    {
        try
        {
            _logger.LogInformation("Gateway: Buscando notificação {Id}", id);

            var httpClient = _httpClientFactory.CreateClient("NotificationAPI");
            var response = await httpClient.GetAsync($"api/notification/{id}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonSerializer.Deserialize<object>(result));
            }

            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao buscar notificação");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(string userId)
    {
        try
        {
            _logger.LogInformation("Gateway: Buscando notificações do usuário {UserId}", userId);

            var httpClient = _httpClientFactory.CreateClient("NotificationAPI");
            var response = await httpClient.GetAsync($"api/notification/user/{userId}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonSerializer.Deserialize<object>(result));
            }

            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao buscar notificações do usuário");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessNotification(string id)
    {
        try
        {
            _logger.LogInformation("Gateway: Processando notificação {Id}", id);

            var httpClient = _httpClientFactory.CreateClient("NotificationAPI");
            var response = await httpClient.PostAsync($"api/notification/{id}/process", null);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonSerializer.Deserialize<object>(result));
            }

            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao processar notificação");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelNotification(string id)
    {
        try
        {
            _logger.LogInformation("Gateway: Cancelando notificação {Id}", id);

            var httpClient = _httpClientFactory.CreateClient("NotificationAPI");
            var response = await httpClient.DeleteAsync($"api/notification/{id}");

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Notificação cancelada com sucesso" });
            }

            var result = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao cancelar notificação");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}

public class NotificationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
} 