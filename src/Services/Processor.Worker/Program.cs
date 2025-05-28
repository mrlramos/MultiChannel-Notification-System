using Processor.Worker.Workers;
using Processor.Worker.Services;
using Processor.Worker.Providers;
using Serilog;
using Polly;
using Polly.Extensions.Http;

var builder = Host.CreateApplicationBuilder(args);

// Configurar Serilog
builder.Services.AddSerilog((services, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration));

// Configurar HttpClient com Polly
builder.Services.AddHttpClient<INotificationProcessor, NotificationProcessor>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
//.AddPolicyHandler(GetRetryPolicy())
//.AddPolicyHandler(GetCircuitBreakerPolicy());

// Configurar HttpClients para provedores
builder.Services.AddHttpClient<EmailProvider>();
builder.Services.AddHttpClient<SmsProvider>();
builder.Services.AddHttpClient<PushProvider>();

// Registrar serviços
builder.Services.AddScoped<INotificationProcessor, NotificationProcessor>();

// Registrar provedores
builder.Services.AddScoped<IEmailProvider, EmailProvider>();
builder.Services.AddScoped<ISmsProvider, SmsProvider>();
builder.Services.AddScoped<IPushProvider, PushProvider>();

// Registrar Worker
builder.Services.AddHostedService<NotificationWorker>();

// Configurar Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("notification-processor", () => 
    {
        // Health check básico
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Worker está funcionando");
    });

var host = builder.Build();

// Log de inicialização
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Iniciando MultiChannel Notification Processor Worker");
logger.LogInformation("📋 Configuração:");
logger.LogInformation("   - Service Bus: {ServiceBus}", 
    builder.Configuration.GetConnectionString("ServiceBus")?.Contains("localhost") == true ? "Simulação" : "Azure");
logger.LogInformation("   - Notification API: {NotificationAPI}", 
    builder.Configuration["Services:NotificationAPI"]);
logger.LogInformation("   - Subscription API: {SubscriptionAPI}", 
    builder.Configuration["Services:SubscriptionAPI"]);

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Falha crítica na aplicação");
    throw;
}
finally
{
    logger.LogInformation("🛑 Processor Worker finalizado");
    Log.CloseAndFlush();
}

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"🔄 HTTP Retry {retryCount} em {timespan.TotalSeconds}s: {outcome.Exception?.Message}");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (exception, duration) =>
            {
                Log.Warning("🔌 Circuit Breaker ABERTO por {Duration}s: {Exception}", 
                    duration.TotalSeconds, exception.Exception?.Message);
            },
            onReset: () =>
            {
                Log.Information("🔌 Circuit Breaker FECHADO - Conexões restauradas");
            });
} 