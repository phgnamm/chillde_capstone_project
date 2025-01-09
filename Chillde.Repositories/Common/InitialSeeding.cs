using Chillde.Repositories.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Chillde.Repositories.Common;

/// <summary>
///     This class is used to insert initial data
/// </summary>
public static class InitialSeeding
{
    private static readonly List<Role> Roles = new()
    {
        new() { Name = Enums.Role.Admin.ToString() },
        new() { Name = Enums.Role.Customer.ToString() },
        new() { Name = Enums.Role.Artist.ToString() }
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
            HashedPassword = "Test",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            Status = Enums.AccountStatus.Active,
            WalletId = Guid.Parse("f335111a-1b3c-485f-aa87-ebbf689dbd4c")
        }
    };

    private static readonly List<Category> Categories = new()
    {
        new() { Id = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"), Code = "C01", Name = "Accessories" },
        new() { Id = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"), Code = "C02", Name = "Art & Collectibles" },
        new() { Id = Guid.Parse("b698f941-943c-47d1-88fd-9e0e05f6e15a"), Code = "C03", Name = "Jewelry" }
    };

    private static readonly List<SubCategory> SubCategories = new()
    {
            new() { Id = Guid.Parse("f2ef8608-50f5-48e5-a6a3-51e2dbdf065b"), Code = "SC01", Name = "Belts & Suspenders", CategoryId = Categories[0].Id},
            new() { Id = Guid.Parse("85a007bc-b215-46fe-98bc-eac1d44e2234"), Code = "SC02", Name = "Sunglasses & Eyewear", CategoryId = Categories[0].Id},
            new() { Id = Guid.Parse("f576216f-b84c-4b71-adff-0d67f21ae7a3"), Code = "SC03", Name = "Bouquets & Corsages", CategoryId = Categories[0].Id},
            new() { Id = Guid.Parse("723b7ac6-ea6f-4ba0-b456-2bd6ea225e3e"), Code = "SC04", Name = "Prints", CategoryId = Categories[1].Id},
            new() { Id = Guid.Parse("19001a42-5d0b-49f1-be6a-d975145d2c48"), Code = "SC05", Name = "Sculpture", CategoryId = Categories[1].Id},
            new() { Id = Guid.Parse("e90ad4b0-2c62-4614-9a8f-307371d8420e"), Code = "SC06", Name = "Painting", CategoryId = Categories[1].Id},
            new() { Id = Guid.Parse("f48bec9d-c653-4952-973a-72c20e07341d"), Code = "SC07", Name = "Necklaces", CategoryId = Categories[2].Id},
            new() { Id = Guid.Parse("ba815129-7ad6-4d2d-ba7b-a43977bc310e"), Code = "SC08", Name = "Earrings", CategoryId = Categories[2].Id},
            new() { Id = Guid.Parse("ec15a653-a1d9-4926-b8b5-79b556350928"), Code = "SC09", Name = "Rings", CategoryId = Categories[2].Id},
    };

    private static readonly List<Item> Items = new()
    {
        new() { Id = Guid.Parse("069d4f6f-78ab-4bf1-a63e-b6ce32cb3ff3"), Code = "I01", Name = "Belts", SubCategoryId = SubCategories[0].Id},
        new() { Id = Guid.Parse("d6bc3741-39b4-4298-9261-1e1108872592"), Code = "I02", Name = "Suspenders", SubCategoryId = SubCategories[0].Id},
        new() { Id = Guid.Parse("afc1a6ae-7000-4438-980f-63127ef9c41f"), Code = "I03", Name = "Sunglasses", SubCategoryId = SubCategories[1].Id},
        new() { Id = Guid.Parse("f4332eb9-2ea2-4155-9456-362bfb43b291"), Code = "I04", Name = "Glasses", SubCategoryId = SubCategories[1].Id},
        new() { Id = Guid.Parse("54b19665-727a-4447-b259-f831ee0de5f6"), Code = "I05", Name = "Glasses Cases", SubCategoryId = SubCategories[1].Id},
        new() { Id = Guid.Parse("5385ed2d-2046-4400-9f25-78ad964102e1"), Code = "I06", Name = "Digital Prints", SubCategoryId = SubCategories[3].Id},
        new() { Id = Guid.Parse("f068175a-67ea-4c91-94e6-4ec1022e949a"), Code = "I07", Name = "Y Necklaces", SubCategoryId = SubCategories[6].Id},
    };

    private static readonly List<Service> Services = new()
    {
        new() 
        { 
            Id = Guid.Parse("1844b073-513f-4bd3-9fc3-8c77422cf22c"), 
            Name = "Làm dây chuyền chữ Y", 
            Description = "Thiết kế và tạo một chiếc dây đeo hoàn toàn mới theo yêu cầu của khách hàng.",
            IsOffter = true,
            Status = Enums.ServiceStatus.Active,
            ItemId = Items[6].Id,
            CreatedById = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b")
        },
    };

    private static readonly List<Package> Packages = new()
    {
        new()
        {
            Id = Guid.Parse("22e1e7a3-bc97-438c-a36e-30379874165c"),
            Name = "Basic",
            Description = "",
            Price = 2000000,
            ServiceId = Services[0].Id
        },
        new()
        {
            Id = Guid.Parse("8fe3412e-e6cb-41e3-b43d-5a72cddd98f3"),
            Name = "Plus",
            Description = "",
            Price = 5000000,
            ServiceId = Services[0].Id
        },
        new()
        {
            Id = Guid.Parse("c68703b7-6ebc-4b98-aa35-b48d5452878f"),
            Name = "Premium",
            Description = "",
            Price = 10000000,
            ServiceId = Services[0].Id
        },
    };

    private static readonly List<Feature> Features = new()
    {
        new(){ Id = Guid.Parse("e51ae29b-e048-4db0-89a2-670a44e6eea7"), Name = "Thiết kế theo yêu cầu" },
        new(){ Id = Guid.Parse("f335111a-1b3c-485f-aa87-ebbf689dbd4c"), Name = "Khắc tên hoặc ký tự đặc biệt" },
        new(){ Id = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b"), Name = "Sơn màu lên sản phẩm" },
        new(){ Id = Guid.Parse("6551bec5-12eb-49e1-ba72-134545db85dc"), Name = "Đính đá zirconia nhỏ" },
    };

    private static readonly List<PackageFeature> PackageFeatures = new()
    {
        new()
        {
            Id = Guid.Parse("9117fe29-9fec-4cf5-8f52-37d1efec7b22"),
            Question = "Thiết kế theo yêu cầu",
            IsInformationRequired = false,
            IsExtra = false,
            AdditionalCost = null,
            AdditionalDay = null,
            PackageId = Packages[0].Id,
            FeatureId = Features[0].Id
        },
        new()
        {
            Id = Guid.Parse("fbfd784f-bfe3-49ed-99b5-19c46c19b6f1"),
            Question = "Khắc tên hoặc ký tự đặc biệt (5 ký tự miễn phí)",
            IsInformationRequired = false,
            IsExtra = false,
            AdditionalCost = null,
            AdditionalDay = null,
            PackageId = Packages[0].Id,
            FeatureId = Features[1].Id
        },
        new()
        {
            Id = Guid.Parse("4871b8c7-af47-4bb6-8fad-226fb5e84d2e"),
            Question = "Sơn màu lên sản phẩm (2 màu tùy chọn)",
            IsInformationRequired = false,
            IsExtra = false,
            AdditionalCost = null,
            AdditionalDay = null,
            PackageId = Packages[0].Id,
            FeatureId = Features[2].Id
        },
        new()
        {
            Id = Guid.Parse("a03772d4-9c79-406c-8b1a-6c586df22b15"),
            Question = "Đính đá zirconia nhỏ (tối đa 5 viên miễn phí)",
            IsInformationRequired = false,
            IsExtra = false,
            AdditionalCost = null,
            AdditionalDay = null,
            PackageId = Packages[0].Id,
            FeatureId = Features[3].Id
        },
        new()
        {
            Id = Guid.Parse("6c8d8723-d2b1-48fd-8fea-46d3b70782e1"),
            Question = "Khắc thêm ký tự đặc biệt",
            IsInformationRequired = false,
            IsExtra = true,
            AdditionalCost = 5000,
            AdditionalDay = null,
            PackageId = Packages[1].Id,
            FeatureId = Features[1].Id
        },
        new()
        {
            Id = Guid.Parse("7b218041-fe35-4c61-a6c0-83d887bbb67e"),
            Question = "Khắc thêm ký tự đặc biệt được thiết kế riêng",
            IsInformationRequired = false,
            IsExtra = true,
            AdditionalCost = 10000,
            AdditionalDay = null,
            PackageId = Packages[2].Id,
            FeatureId = Features[1].Id
        },
    };

    /// <summary>
    /// Initialize and seed the database with roles, categories, and subcategories.
    /// </summary>
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

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

        // Seed Categories
        foreach (var category in Categories)
        {
            if (!context.Categories.Any(c => c.Id == category.Id))
            {
                category.CreationDate = DateTime.UtcNow;
                context.Categories.Add(category);
            }
        }

        foreach (var subCategory in SubCategories)
        {
            if (!context.SubCategories.Any(sc => sc.Id == subCategory.Id))
            {
                subCategory.CreationDate = DateTime.UtcNow;
                context.SubCategories.Add(subCategory);
            }
        }

        foreach (var item in Items)
        {
            if (!context.Items.Any(i => i.Id == item.Id))
            {
                item.CreationDate = DateTime.UtcNow;
                context.Items.Add(item);
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


        await context.SaveChangesAsync();
    }
}
