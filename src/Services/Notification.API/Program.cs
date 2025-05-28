using Serilog;
using Notification.API.Services;
using Notification.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configurar URLs
builder.WebHost.UseUrls("http://0.0.0.0:80");

// Configurar Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Adicionar serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "MultiChannel Notification API", 
        Version = "v1",
        Description = "API principal para processamento e envio de notificações"
    });
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Adicionar Health Checks
builder.Services.AddHealthChecks();

// Registrar serviços de domínio (usando implementações em memória)
builder.Services.AddSingleton<INotificationRepository, InMemoryNotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageQueueService, InMemoryMessageQueueService>();

var app = builder.Build();

// Pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHealthChecks("/health");
app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
