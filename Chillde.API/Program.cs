using System.Globalization;
using System.Text.Json.Serialization;
using Chillde.API;
using Chillde.API.Middlewares;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Services.Hubs;
using Chillde.Services.Services;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Nest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Ignore all fields with null value in response
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddSignalR(options => { options.MaximumReceiveMessageSize = null; });
var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"] ?? "https://localhost:9200";
var username = builder.Configuration["Elasticsearch:Username"] ?? "elastic";
var password = builder.Configuration["Elasticsearch:Password"] ?? "l4*-7pKHsSIH0LgjIy=9";
// ?? Register `ElasticClient` in DI Container
var settings = new ConnectionSettings(new Uri(elasticsearchUrl))
    .BasicAuthentication(username, password) // ? Add Authentication
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll) // ? Ignore SSL errors if needed
    .DefaultIndex("services");
var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = builder.Configuration["JWT:ValidAudience"], Version = "v1" });
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    x.OperationFilter<AcceptLanguageHeaderFilter>();
});

// Add API configuration
builder.Services.AddApiConfiguration(builder.Configuration);

//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
//        options.JsonSerializerOptions.MaxDepth = 64; // optional: to avoid issues with deeply nested objects
//    });

//Session
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30); // Token GHTK có thể hết hạn sau 30 phút
    options.Cookie.HttpOnly = true; // Bảo mật hơn
    options.Cookie.IsEssential = true;
});

//builder.Services.AddHostedService<WorkerService>();
// builder.Services.AddHostedService<OrderTrackingReminderService>();
// builder.Services.AddHostedService<OrderReminderService>();
var app = builder.Build();

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

// Allow CORS
app.UseCors("cors");



// Initial seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await InitialSeeding.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AccountStatusMiddleware>();

app.UseSession();
//app.UseStaticFiles();

app.MapControllers();
app.MapHub<RealTimeHub>("/hub");

app.Run();
