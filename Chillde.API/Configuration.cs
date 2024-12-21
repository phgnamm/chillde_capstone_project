using System.Diagnostics;
using System.Text;
using Chillde.API.Middlewares;
using Chillde.API.Utils;
using Chillde.Repositories;
using Chillde.Repositories.Common;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Repositories;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Chillde.API;

public static class Configuration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        #region Configuartion

        // Local database
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("LocalDb"));
        });

        // Redis
        var redisConfiguration = configuration["Redis:Configuration"];
        ArgumentException.ThrowIfNullOrWhiteSpace(redisConfiguration);
        services.AddStackExchangeRedisCache(options => { options.Configuration = redisConfiguration; });
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConfiguration));

        // Cloudinary
        var cloud = configuration["Cloudinary:Cloud"];
        ArgumentException.ThrowIfNullOrWhiteSpace(cloud);
        var apiKey = configuration["Cloudinary:ApiKey"];
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        var apiSecret = configuration["Cloudinary:ApiSecret"];
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        var cloudinary = new Cloudinary(new Account { Cloud = cloud, ApiKey = apiKey, ApiSecret = apiSecret });
        services.AddSingleton<ICloudinary>(cloudinary);

        // JWT
        var secret = configuration["JWT:Secret"];
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        var issuer = configuration["JWT:ValidIssuer"];
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        var audience = configuration["JWT:ValidAudience"];
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hub"))
                        context.Token = accessToken;

                    return Task.CompletedTask;
                }
            };
        });

        // CORS
        var clientUrl = configuration["URL:Client"];
        ArgumentException.ThrowIfNullOrWhiteSpace(clientUrl);
        services.AddCors(options =>
        {
            options.AddPolicy("cors",
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder
                        .WithOrigins(clientUrl)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        #endregion

        #region Middleware

        services.AddScoped<AccountStatusMiddleware>();
        services.AddSingleton<GlobalExceptionMiddleware>();
        services.AddSingleton<PerformanceMiddleware>();
        services.AddSingleton<Stopwatch>();

        #endregion

        #region Common

        services.AddHttpContextAccessor();
        services.AddAutoMapper(typeof(MapperProfile).Assembly);
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        #endregion

        # region Helper

        services.AddScoped<ICloudinaryHelper, CloudinaryHelper>();
        services.AddTransient<IEmailHelper, EmailHelper>();
        services.AddScoped<IRedisHelper, RedisHelper>();

        #endregion

        #region Dependency Injection

        // Account
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAccountRepository, AccountRepository>();

        // AccountConversation
        services.AddScoped<IAccountConversationRepository, AccountConversationRepository>();

        // AccountRole
        services.AddScoped<IAccountRoleRepository, AccountRoleRepository>();

        // Conversation
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        // Message
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        // MessageRecipient
        services.AddScoped<IMessageRecipientRepository, MessageRecipientRepository>();

        // RefreshToken
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Role
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Request
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IRequestService, RequestService>();

        //RequestDetail
        services.AddScoped<IRequestDetailRepository, RequestDetailRepository>();


        //Order
        services.AddScoped<IOrderRepository, OrderRepository>();

        //Service
        services.AddScoped<IServiceRepository, ServiceRepository>();

        //Package
        services.AddScoped<IPackageRepository, PackageRepository>();

        //Feedback
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IFeedbackService, FeedbackService>();

        //FeedbackImage
        services.AddScoped<IFeedbackImageRepository, FeedbackImageRepository>();

        //Wallet
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletService, WalletService>();

        //WalletHistory
        services.AddScoped<IWalletHistoryRepository, WalletHistoryRepository>();
        services.AddScoped<IWalletHistoryService, WalletHistoryService>();

        #endregion

        return services;
    }
}