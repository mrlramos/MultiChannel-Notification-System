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

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Recebendo requisição para enviar notificação para {UserId}", request.UserId);

            var httpClient = _httpClientFactory.CreateClient("NotificationAPI");
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("api/notification", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Ok(new { message = "Notificação enviada com sucesso", data = result });
            }

            return StatusCode((int)response.StatusCode, "Erro ao enviar notificação");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar requisição de notificação");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("user/{userId}/preferences")]
    public async Task<IActionResult> GetUserPreferences(string userId)
    {
        try
        {
            _logger.LogInformation("Buscando preferências do usuário {UserId}", userId);

            var httpClient = _httpClientFactory.CreateClient("SubscriptionAPI");
            var response = await httpClient.GetAsync($"api/subscription/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            return StatusCode((int)response.StatusCode, "Erro ao buscar preferências");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar preferências do usuário");
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
    public Dictionary<string, object>? Metadata { get; set; }
} 