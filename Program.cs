using VirtualQueueApi.Hubs;
using sib_api_v3_sdk.Client;
using VirtualQueueApi.Configuration;
using Stripe;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var brevoApiKey = builder.Configuration["BrevoService:ApiKey"];
Configuration.Default.ApiKey.Add("api-key", brevoApiKey);

var stripeApiKey = builder.Configuration["StrikeService:ApiKey"];
StripeConfiguration.ApiKey = stripeApiKey;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // Apenas para facilitar leitura no retorno
    });// Ensure this line is present

builder.Services.AddIdentityConfiguration(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program));  // Isso registra todos os profiles automaticamente
builder.Services.RegisterServices();

builder.Services.AddApiConfiguration(builder.Configuration);

builder.Services.AddRateLimitingConfiguration();
builder.Services.AddExceptionHandlerConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseApiConfiguration();
app.UseRateLimitingConfiguration();

app.UseIdentityConfiguration();

app.UseExceptionHandlerConfiguration();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<QueueHub>("/queueHub");
});

app.Run();