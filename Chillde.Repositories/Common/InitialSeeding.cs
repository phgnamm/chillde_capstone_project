using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Chillde.Repositories.Common;

/// <summary>
///     This class is used to insert initial data
/// </summary>
public static class InitialSeeding
{

    private static readonly List<Entities.Role> Roles = new()
    {
        new() { Name = Enums.Role.Admin.ToString() },
        new() { Name = Enums.Role.Customer.ToString() },
        new() { Name = Enums.Role.Artisan.ToString() }
    };

    private static readonly List<Wallet> Wallets = new()
    {
        new() {Id = Guid.Parse("f335111a-1b3c-485f-aa87-ebbf689dbd4c"), Balance = 1000000000000, CreatedById = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b")}
    };

    private static readonly List<Account> Accounts = new()
    {
        new()
        {
            Id = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b"),
            FirstName = "Test",
            LastName = "Test",
            Username = "Test",
            Email = "Test",
            PhoneNumber = "0975993464",
            HashedPassword = "Test",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            Status = Enums.AccountStatus.Active,
            WalletId = Guid.Parse("f335111a-1b3c-485f-aa87-ebbf689dbd4c")
        }
    };

    private static readonly List<Category> Categories = new()
    {
        new()
        {
            Id = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"),
            Name = "Accessories",
        },
        new()
        {
            Id = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
            Name = "Art & Collectibles"
        },
        new()
        { 
            Id = Guid.Parse("b698f941-943c-47d1-88fd-9e0e05f6e15a"),
            Name = "Jewelry"
        },
        new() 
        { 
            Id = Guid.Parse("f2ef8608-50f5-48e5-a6a3-51e2dbdf065b"),
            Name = "Belts & Suspenders", 
            ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b")
        },
        new() 
        { 
            Id = Guid.Parse("85a007bc-b215-46fe-98bc-eac1d44e2234"), 
            Name = "Sunglasses & Eyewear",
            ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b")
        },
        new() 
        { 
            Id = Guid.Parse("f576216f-b84c-4b71-adff-0d67f21ae7a3"),
            Name = "Bouquets & Corsages",
            ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b")
        },
        new() 
        { 
            Id = Guid.Parse("723b7ac6-ea6f-4ba0-b456-2bd6ea225e3e"),
            Name = "Prints",
            ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52")
        },
        new() 
        { 
            Id = Guid.Parse("ba815129-7ad6-4d2d-ba7b-a43977bc310e"),
            Name = "Earrings",
            ParentId = Guid.Parse("b698f941-943c-47d1-88fd-9e0e05f6e15a")
        },
    };

    //private static readonly List<C> SubCategories = new()
    //{
            
            
            
            
    //        new() { Id = Guid.Parse("19001a42-5d0b-49f1-be6a-d975145d2c48"), Code = "SC05", Name = "Sculpture", CategoryId = Categories[1].Id},
    //        new() { Id = Guid.Parse("e90ad4b0-2c62-4614-9a8f-307371d8420e"), Code = "SC06", Name = "Painting", CategoryId = Categories[1].Id},
    //        new() { Id = Guid.Parse("f48bec9d-c653-4952-973a-72c20e07341d"), Code = "SC07", Name = "Necklaces", CategoryId = Categories[2].Id},
            
    //        new() { Id = Guid.Parse("ec15a653-a1d9-4926-b8b5-79b556350928"), Code = "SC09", Name = "Rings", CategoryId = Categories[2].Id},
    //};


    private static readonly List<SystemConfig> systemConfigs = new()
    {
        new() { EntityType = ConfigType.Security, FieldName = "AccessTokenValidityInMinutes", Value = JsonDocument.Parse("5") },
        new() { EntityType = ConfigType.Security, FieldName = "RefreshTokenValidityInDays", Value = JsonDocument.Parse("7") },
        new() { EntityType = ConfigType.Security, FieldName = "VerificationCodeValidityInMinutes", Value = JsonDocument.Parse("15") },
        new() { EntityType = ConfigType.Security, FieldName = "VerificationCodeLength", Value = JsonDocument.Parse("6") },
        new() { EntityType = ConfigType.Security, FieldName = "ResetPasswordTokenValidityInMinutes", Value = JsonDocument.Parse("15") },
        new() { EntityType = ConfigType.Pagination, FieldName = "DefaultMinPageSize", Value = JsonDocument.Parse("10") },
        new() { EntityType = ConfigType.Pagination, FieldName = "DefaultMaxPageSize", Value = JsonDocument.Parse("50") },
        new() { EntityType = ConfigType.Pagination, FieldName = "ConversationMaxPageSize", Value = JsonDocument.Parse("20") },
        new() { EntityType = ConfigType.Pagination, FieldName = "MessageMinPageSize", Value = JsonDocument.Parse("10") },
        new() { EntityType = ConfigType.Pagination, FieldName = "MessageMaxPageSize", Value = JsonDocument.Parse("100") },
        new() { EntityType = ConfigType.Cache, FieldName = "DefaultAbsoluteExpirationInMinutes", Value = JsonDocument.Parse("60") },
        new() { EntityType = ConfigType.Cache, FieldName = "DefaultSlidingExpirationInMinutes", Value = JsonDocument.Parse("30") },
        new() { EntityType = ConfigType.Package, FieldName = "MaximumPackageOfOneService", Value = JsonDocument.Parse("3") },
        new() { EntityType = ConfigType.Package, FieldName = "MaximumFeatureOfOnePackage", Value = JsonDocument.Parse("15") },
        new() { EntityType = ConfigType.Order, FieldName = "MaxSystemCancelPerYear", Value = JsonDocument.Parse("10") },
        new() { EntityType = ConfigType.Order, FieldName = "MaxSystemCancelBeforePenalty", Value = JsonDocument.Parse("3") },
        new() { EntityType = ConfigType.Order, FieldName = "PenaltyPercentageAfterCancel", Value = JsonDocument.Parse("20") },
        new() { EntityType = ConfigType.Order, FieldName = "OrderSuccessThreshold", Value = JsonDocument.Parse("3") },
        new() { EntityType = ConfigType.Reputation, FieldName = "MaxOrderPerMonthBasedOnReputation", Value = JsonDocument.Parse("6") },
        new() { EntityType = ConfigType.Reputation, FieldName = "MinReputationForVouchers", Value = JsonDocument.Parse("6") },
        new() { EntityType = ConfigType.Order, FieldName = "MaxCustomerOrdersPerMonth", Value = JsonDocument.Parse("3") },
        new() { EntityType = ConfigType.Reputation, FieldName = "ReputationIncreaseOnSuccess", Value = JsonDocument.Parse("2") },
        new() { EntityType = ConfigType.Reputation, FieldName = "MinReputationToAvoidBan", Value = JsonDocument.Parse("3") },

    };
    private static readonly List<Service> Services = new()
    {
        new()
        {
            Id = Guid.Parse("1844b073-513f-4bd3-9fc3-8c77422cf22c"),
            Name = "Làm dây chuyền chữ Y",
            Description = "Thiết kế và tạo một chiếc dây đeo hoàn toàn mới theo yêu cầu của khách hàng.",
            Status = Enums.ServiceStatus.Active,
            CategoryId = Categories[0].Id,
            CreatedById = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b"),
        },
    };

    private static readonly List<Package> Packages = new()
    {
        new()
        {
            Id = Guid.Parse("22e1e7a3-bc97-438c-a36e-30379874165c"),
            Name = PackageName.Basic,
            Description = "",
            Price = 2000000,
            SketchRevision = 1,
            ServiceId = Services[0].Id
        },
        new()
        {
            Id = Guid.Parse("8fe3412e-e6cb-41e3-b43d-5a72cddd98f3"),
            Name = PackageName.Standard,
            Description = "",
            Price = 5000000,
            SketchRevision = 2,
            ServiceId = Services[0].Id
        },
        new()
        {
            Id = Guid.Parse("c68703b7-6ebc-4b98-aa35-b48d5452878f"),
            Name = PackageName.Premium,
            Description = "",
            Price = 10000000,
            SketchRevision = 3,
            ServiceId = Services[0].Id
        },
    };

    private static readonly List<Feature> Features = new()
    {
        new()
        {
            Id = Guid.Parse("e51ae29b-e048-4db0-89a2-670a44e6eea7"),
            Name = "Thiết kế theo yêu cầu",
            Question = "Bạn muốn thiết kế theo kiểu như thế nào cho sản phẩm?",
            QuestionType = MediaType.Text,
            IsInformationRequired = true,
            IsQuantity = false
        },
        new()
        {
            Id = Guid.Parse("f335111a-1b3c-485f-aa87-ebbf689dbd4c"),
            Name = "Khắc tên hoặc ký tự đặc biệt",
            Question = "Bạn muốn có muốn khắc ký tự đặc biệt không?",
            QuestionType = MediaType.Text,
            IsInformationRequired = false,
            IsQuantity = false
        },
        new()
        {
            Id = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b"),
            Name = "Sơn màu lên sản phẩm",
            Question = "Bạn muốn sản phẩm màu gì?",
            QuestionType = MediaType.Select,
            IsInformationRequired = true,
            IsQuantity = false
        },
        new()
        {
            Id = Guid.Parse("6551bec5-12eb-49e1-ba72-134545db85dc"),
            Name = "Đính đá zirconia nhỏ",
            Question = "Bạn muốn thêm đá zirconia nhỏ",
            QuestionType = MediaType.Switch,
            IsInformationRequired = true,
            IsQuantity = true
        },
    };

    private static readonly List<PackageFeature> PackageFeatures = new()
    {
        new()
        {
            Id = Guid.Parse("9117fe29-9fec-4cf5-8f52-37d1efec7b22"),
            Name = "Thiết kế theo yêu cầu",
            IsExtra = false,
            AdditionalCost = null,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 1,
            PackageId = Packages[0].Id,
            FeatureId = Features[0].Id
        },
        new()
        {
            Id = Guid.Parse("fbfd784f-bfe3-49ed-99b5-19c46c19b6f1"),
            Name = "Khắc tên hoặc ký tự đặc biệt (5 ký tự miễn phí)",
            IsExtra = false,
            AdditionalCost = null,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 1,
            PackageId = Packages[0].Id,
            FeatureId = Features[1].Id
        },
        new()
        {
            Id = Guid.Parse("4871b8c7-af47-4bb6-8fad-226fb5e84d2e"),
            Name = "Sơn màu lên sản phẩm (2 màu tùy chọn)",
            IsExtra = false,
            AdditionalCost = null,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 1,
            PackageId = Packages[0].Id,
            FeatureId = Features[2].Id
        },
        new()
        {
            Id = Guid.Parse("a03772d4-9c79-406c-8b1a-6c586df22b15"),
            Name = "Đính đá zirconia nhỏ (tối đa 5 viên miễn phí)",
            IsExtra = false,
            AdditionalCost = null,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 1,
            PackageId = Packages[0].Id,
            FeatureId = Features[3].Id
        },
        new()
        {
            Id = Guid.Parse("6c8d8723-d2b1-48fd-8fea-46d3b70782e1"),
            Name = "Khắc thêm ký tự đặc biệt",
            IsExtra = true,
            AdditionalCost = 5000,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 1,
            PackageId = Packages[1].Id,
            FeatureId = Features[1].Id
        },
        new()
        {
            Id = Guid.Parse("7b218041-fe35-4c61-a6c0-83d887bbb67e"),
            Name = "Khắc thêm ký tự đặc biệt được thiết kế riêng",
            IsExtra = true,
            AdditionalCost = 10000,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 1,
            PackageId = Packages[2].Id,
            FeatureId = Features[1].Id
        },
    };
    private static readonly List<Order> Orders = new()
    {
        new()
        {
        Id = Guid.Parse("4871b8c7-af46-4bb6-8fad-226fb5e84d2e"),
        Code = "ORDCHD125FG8G1",
        Phone = "0912345678",
        Address = "123 Fake Street, City, Country",
        ToWard = "20308",
        ToDistrict = 1444,
        ToProvince = "HCM",
        PackagePrice = 2500000,
        Quantity = 2,
        ShipmentCode = "ORDCHD125FG8G1_Delivery",
        CancleOrderReason = CancleOrderReason.ChangeShipmentAddress,
        DeliveryTime = DateTime.UtcNow.AddDays(7),
        CurrentSketchRevision = 1,
        Stage = OrderStage.Shipping,
        Status = OrderStatus.Pending,
        TotalValue = 2500000,
        FinalValue = 2500000,
        AdminCommission = 500000,
        ArtistRevenue = 2000000,
        VoucherCost = 0,
        PackageId = Packages[0].Id,
        CreatedById = Accounts[0].Id,
        Payments = new List<Payment>
        {
            new Payment
            {
                Id = Guid.Parse("f07caba7-842c-46be-acef-fcb6a5d20fd0"),
                PaymentType = PaymentType.VnPay,
                Amount = 2500000,
                PaymentStatus = PaymentStatus.Success
            }
        },
        OrderInformations = new List<OrderInformation>
        {
            new OrderInformation
            {
                Id = Guid.Parse("d082cfcd-b5d9-4c1a-9f18-d8a0a6113bcc"),
                Description = "Order Information for Product A",
                Price = 2500000,
                PackageFeatureId = PackageFeatures[0].Id,
                OrderInformationAttachments = new List<OrderInformationAttachment>
                {
                    new OrderInformationAttachment
                    {
                        Id = Guid.Parse("d4675207-1a94-4493-8e5f-f2672d31dd52"),
                        AttachmentUrl = "path/to/file.jpg",
                        AttachmentAlt = "image",
                    }
                }
            }
        }
    },
    new()
    {
        Id = Guid.Parse("9a1ed03d-ae8c-4b71-8637-7b35f83316c3"),
        Phone = "0987654321",
        Code = "ORDCHD126FG8G2",
        Address = "456 Another Street, City, Country",
        ToWard = "20308",
        ToDistrict = 1444,
        ToProvince = "HCM",
        PackagePrice = 2500000,
        Quantity = 1,
        ShipmentCode = "ORDCHD125FG8G1_Delivery",
        CancleOrderReason = CancleOrderReason.ChangeShipmentAddress,
        DeliveryTime = DateTime.UtcNow.AddDays(7),
        CurrentSketchRevision = 1,
        Stage = OrderStage.Shipping,
        Status = OrderStatus.Pending,
        TotalValue = 2500000,
        FinalValue = 2500000,
        AdminCommission = 500000,
        ArtistRevenue = 2000000,
        VoucherCost = 0,
        PackageId = Packages[0].Id,
        CreatedById = Accounts[0].Id,
        Payments = new List<Payment>
        {
            new Payment
            {
                Id = Guid.Parse("7604cdf4-3fa1-464c-824d-9b89e567ee8c"),
                PaymentType = PaymentType.VnPay,
                Amount = 100.75m,
                PaymentStatus = PaymentStatus.Success
            }
        },
        OrderInformations = new List<OrderInformation>
        {
            new OrderInformation
            {
                Id = Guid.Parse("753a3b34-d33e-479b-8940-da24f2a5bacd"),
                Description = "Order Information for Product B",
                PackageFeatureId = PackageFeatures[1].Id,
                Price = 2500000,
                OrderInformationAttachments = new List<OrderInformationAttachment>
                {
                    new OrderInformationAttachment
                    {
                        Id = Guid.Parse("dca69da4-8772-4d70-9736-a59e49a61e5f"),
                        AttachmentUrl = "path/to/attachment.pdf",
                        AttachmentAlt = "pdf",
                    }
                }
      }
    }
    }
    };

    // This assumes `Packages`, `Shipments`, `Accounts`, `PackageFeatures`, and other related data exist

    /// <summary>
    /// Initialize and seed the database with roles, categories, and subcategories.
    /// </summary>
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        foreach (var config in systemConfigs)
        {
            if (!context.SystemConfigs.Any(c => c.EntityType == config.EntityType && c.FieldName == config.FieldName))
            {
                config.CreationDate = DateTime.UtcNow;
                context.SystemConfigs.Add(config);
            }
        }
        // Seed Roles
        foreach (var role in Roles)
        {
            if (!context.Roles.Any(r => r.Name == role.Name))
            {
                role.CreationDate = DateTime.UtcNow;
                context.Roles.Add(role);
            }
        }

        foreach (var wallet in Wallets)
        {
            if (!context.Wallets.Any(c => c.Id == wallet.Id))
            {
                wallet.CreationDate = DateTime.UtcNow;
                context.Wallets.Add(wallet);
            }
        }

        foreach (var account in Accounts)
        {
            if (!context.Accounts.Any(c => c.Id == account.Id))
            {
                account.CreationDate = DateTime.UtcNow;
                context.Accounts.Add(account);
            }
        }

        foreach (var category in Categories)
        {
            if (!context.Categories.Any(c => c.Id == category.Id))
            {
                category.CreationDate = DateTime.UtcNow;
                context.Categories.Add(category);
            }
        }

        foreach (var service in Services)
        {
            if (!context.Services.Any(i => i.Id == service.Id))
            {
                service.CreationDate = DateTime.UtcNow;
                context.Services.Add(service);
            }
        }

        foreach (var package in Packages)
        {
            if (!context.Packages.Any(i => i.Id == package.Id))
            {
                package.CreationDate = DateTime.UtcNow;
                context.Packages.Add(package);
            }
        }

        foreach (var feature in Features)
        {
            if (!context.Features.Any(i => i.Id == feature.Id))
            {
                feature.CreationDate = DateTime.UtcNow;
                context.Features.Add(feature);
            }
        }

        foreach (var packageFeature in PackageFeatures)
        {
            if (!context.PackageFeatures.Any(i => i.Id == packageFeature.Id))
            {
                packageFeature.CreationDate = DateTime.UtcNow;
                context.PackageFeatures.Add(packageFeature);
            }
        }
        foreach (var order in Orders)
        {
            if (!context.Orders.Any(i => i.Id == order.Id))
            {
                order.CreationDate = DateTime.UtcNow;
                context.Orders.Add(order);
            }
        }


        await context.SaveChangesAsync();
    }
}
