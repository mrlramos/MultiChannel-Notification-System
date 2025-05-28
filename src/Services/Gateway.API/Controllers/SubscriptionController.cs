using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(IHttpClientFactory httpClientFactory, ILogger<SubscriptionController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Gateway: Criando subscrição para {UserId}", request.UserId);

            var httpClient = _httpClientFactory.CreateClient("SubscriptionAPI");
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("api/subscription", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonSerializer.Deserialize<object>(result));
            }

            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao criar subscrição");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetSubscription(string userId)
    {
        try
        {
            _logger.LogInformation("Gateway: Buscando subscrição do usuário {UserId}", userId);

            var httpClient = _httpClientFactory.CreateClient("SubscriptionAPI");
            var response = await httpClient.GetAsync($"api/subscription/user/{userId}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonSerializer.Deserialize<object>(result));
            }

            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao buscar subscrição");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateSubscription(string userId, [FromBody] UpdateSubscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Gateway: Atualizando subscrição do usuário {UserId}", userId);

            var httpClient = _httpClientFactory.CreateClient("SubscriptionAPI");
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"api/subscription/user/{userId}", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonSerializer.Deserialize<object>(result));
            }

            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao atualizar subscrição");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteSubscription(string userId)
    {
        try
        {
            _logger.LogInformation("Gateway: Deletando subscrição do usuário {UserId}", userId);

            var httpClient = _httpClientFactory.CreateClient("SubscriptionAPI");
            var response = await httpClient.DeleteAsync($"api/subscription/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Subscrição deletada com sucesso" });
            }

            var result = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao deletar subscrição");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost("{userId}/validate")]
    public async Task<IActionResult> ValidatePreferences(string userId, [FromBody] ValidatePreferencesRequest request)
    {
        try
        {
            _logger.LogInformation("Gateway: Validando preferências do usuário {UserId}", userId);

            var httpClient = _httpClientFactory.CreateClient("SubscriptionAPI");
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"api/subscription/{userId}/validate", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(JsonSerializer.Deserialize<object>(result));
            }

            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway: Erro ao validar preferências");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}

public class SubscriptionRequest
{
    public string UserId { get; set; } = string.Empty;
    public object Preferences { get; set; } = new();
}

public class UpdateSubscriptionRequest
{
    public object Preferences { get; set; } = new();
}

public class ValidatePreferencesRequest
{
    public string Channel { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
} 