using Serilog;
using Microsoft.Azure.Cosmos;
using Subscription.API.Services;
using Subscription.API.Repositories;

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
        Title = "MultiChannel Subscription API", 
        Version = "v1",
        Description = "API para gerenciamento de preferências e subscrições de usuários"
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

// Configurar Azure Cosmos DB
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetConnectionString("CosmosDB");
    return new CosmosClient(connectionString ?? "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
});

// Registrar serviços de domínio
builder.Services.AddSingleton<ISubscriptionRepository, InMemorySubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

var app = builder.Build();

// Pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Subscription API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHealthChecks("/health");
app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
