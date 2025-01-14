using System.Diagnostics;
using System.Text;
using Chillde.API.Middlewares;
using Chillde.API.Utils;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using OpenAI.GPT3.Extensions;
using Chillde.Services.Common;
using Chillde.Repositories;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Services;
using Chillde.Services.Helpers;
using Chillde.Repositories.Repositories;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

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

        //Translation
        services.AddOpenAIService(settings =>
        {
            settings.ApiKey = configuration["OpenAI:ApiKey"]!;
        });

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
            new CultureInfo("en-US"),
            new CultureInfo("vi-VN")
        };

            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.ApplyCurrentCultureToResponseHeaders = true;

            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
            options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
            options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
        });

        services.AddControllers()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();

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
        // GHNClient 
        services.AddHttpClient("GhnClient", client =>
        {
            client.BaseAddress = new Uri(configuration["GhnSettings:BaseUrl"]!);
            client.DefaultRequestHeaders.Add("Token", configuration["GhnSettings:Token"]);
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
        services.AddScoped<IServiceService, ServiceService>();

        //ServiceAttachment
        services.AddScoped<IServiceAttachmentRepository, ServiceAttachmentRepository>();

        //Package
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IPackageService, PackageService>();

        //Feedback
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IFeedbackService, FeedbackService>();

        //FeedbackImage
        services.AddScoped<IFeedbackAttachmentRepository, FeedbackAttachmentRepository>();

        //Wallet
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletService, WalletService>();

        //WalletHistory
        services.AddScoped<IWalletHistoryRepository, WalletHistoryRepository>();
        services.AddScoped<IWalletHistoryService, WalletHistoryService>();

        //Translation
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<ITranslationRepository, TranslationRepository>();

        //Language
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<ILanguageService, LanguageService>();

        //Offer
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<IOfferService, OfferService>();

        //Caterory
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryServive, CategoryService>();

        //Subcategory
        services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
        services.AddScoped<ISubCategoryService, SubCategoryService>();

        //Item
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IItemService, ItemService>();

        //FAQ
        services.AddScoped<IFAQRepository, FAQRepository>();
        services.AddScoped<IFAQService, FAQService>();

        //Payment
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        //Order
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        //ItemAttribute
        services.AddScoped<IItemAttributeRepository, ItemAttributeRepository>();
        services.AddScoped<IItemAttributeService, ItemAttributeService>();
        //ItemAttributeValue
        services.AddScoped<IAttributeValueRepository, AttributeValueRepository>();
        //Attribute
        services.AddScoped<IAttributeRepository, AttributeRepository>();
        services.AddScoped<IAttributeService, AttributeService>();

        //ShippingAddress
        services.AddScoped<IShippingAddressRepository, ShippingAddressRepository>();
        services.AddScoped<IShippingAddressService, ShippingAddressService>();

        //VnPay
        services.AddSingleton<IVnpay, Vnpay>();
        //PackageFeature
        services.AddScoped<IPackageFeatureRepository, PackageFeatureRepository>();


        //Feature
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IFeatureService, FeatureService>();

        //BadWordFilter
        services.AddScoped<IBadWordFilterService, BadWordFilterService>();

        //PackageFeature
        services.AddScoped<IPackageFeatureRepository, PackageFeatureRepository>();

        //ServiceCollection
        services.AddScoped<IServiceCollectionRepository, ServiceCollectionRepository>();
        services.AddScoped<IServiceCollectionService, ServiceCollectionService>();

        //ServiceWishlist
        services.AddScoped<IServiceWishlistRepository, ServiceWishlistRepository>();
        services.AddScoped<IServiceWishlistService, ServiceWishlistService>();

        //OpenApi
        services.AddTransient<IOpenAiService, OpenAiService>();

        #endregion

        return services;
    }
}