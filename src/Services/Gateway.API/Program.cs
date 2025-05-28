using Serilog;

var builder = WebApplication.CreateBuilder(args);

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
        Title = "MultiChannel Notification Gateway API", 
        Version = "v1",
        Description = "API Gateway para o sistema de notificações multi-canal"
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

// Configurar HttpClient para comunicação com outros serviços
builder.Services.AddHttpClient("NotificationAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotificationAPI"] ?? "http://localhost:5001");
});

builder.Services.AddHttpClient("SubscriptionAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:SubscriptionAPI"] ?? "http://localhost:5002");
});

var app = builder.Build();

// Pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHealthChecks("/health");
app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
