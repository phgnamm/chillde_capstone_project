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
        new() { EntityType = ConfigType.Commission, FieldName = "Commission", Value = JsonDocument.Parse("20") },

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
        new()
        {
            Id = Guid.Parse("5bd4bcfd-9353-4ace-80a5-82d6d313ed59"),
            Name = "Ví da cho ngày lễ tình nhân",
            Description = "Thiết kế ví da hoàn toàn mới theo yêu cầu của khách hàng.",
            Status = Enums.ServiceStatus.Active,
            CategoryId = Categories[0].Id,
            CreatedById = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b"),
        },
    };

    private static readonly List<Package> Packages = new()
    {
        #region Sample Package of service Làm dây chuyền chữ Y (dùng để test)
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
        #endregion

        #region Package of service: "Wallet for Valentine Day"
        new()
        {
            Id = Guid.Parse("fbadecdc-f98e-40cf-877a-9f71dd36be0b"),
            Name = PackageName.Basic,
            Description = "",
            Price = 250000,
            SketchRevision = 1,
            ServiceId = Services[1].Id
        },
        new()
        {
            Id = Guid.Parse("a52c74de-5b24-4a07-b13a-4a725d45a050"),
            Name = PackageName.Standard,
            Description = "",
            Price = 275000,
            SketchRevision = 2,
            ServiceId = Services[1].Id
        },
        new()
        {
            Id = Guid.Parse("e428a1f9-65ec-4324-9cd3-42b3217c23e0"),
            Name = PackageName.Premium,
            Description = "",
            Price = 350000,
            SketchRevision = 3,
            ServiceId = Services[1].Id
        },
#endregion
    };

    private static readonly List<Feature> Features = new()
    {
        #region Sample Feature of service Làm dây chuyền chữ Y (dùng để test)
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
        #endregion

        #region Feature of service: "Wallet for Valentine Day"
        new()
        {
            Id = Guid.Parse("8bb06e3f-17da-47d8-b6f3-44e6325921cc"),
            Name = "Màu",
            Question = "Chọn màu gì?",
            QuestionType = MediaType.Select,
            IsInformationRequired = true,
            IsQuantity = false
        },
        new()
        {
            Id = Guid.Parse("d2ddc903-b467-4867-acf5-ab5a5f187288"),
            Name = "Kích thước",
            Question = "Chọn size gì?",
            QuestionType = MediaType.Select,
            IsInformationRequired = true,
            IsQuantity = false
        },
        new()
        {
            Id = Guid.Parse("220dcbe8-8388-45e9-9a8e-c09fc246abff"),
            Name = "Phụ kiện đi kèm",
            Question = "Muốn đính kèm item gì?",
            QuestionType = MediaType.CheckBox,
            IsInformationRequired = false,
            IsQuantity = true
        },
        new()
        {
            Id = Guid.Parse("3ee74bb9-f578-42cd-b214-eca534f3a89b"),
            Name = "Khoá ví",
            Question = "Muốn có muốn thêm khoá ví?",
            QuestionType = MediaType.Switch,
            IsInformationRequired = false,
            IsQuantity = false
        },
        new()
        {
            Id = Guid.Parse("811219b6-29e6-46e0-96d3-eba76d334895"),
            Name = "Khe đựng thẻ",
            Question = "Muốn có muốn thêm khe đựng thẻ?",
            QuestionType = MediaType.Switch,
            IsInformationRequired = false,
            IsQuantity = true
        },
        new()
        {
            Id = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
            Name = "Khắc ký tự",
            Question = "Muốn có muốn thêm ký tự gì?",
            QuestionType = MediaType.Text,
            IsInformationRequired = true,
            IsQuantity = false
        },
        new()
        {
            Id = Guid.Parse("fcfedfc2-3fbc-41c3-94ad-e922bdbe88e6"),
            Name = "Hình dán ngôi sao",
            Question = "Muốn có muốn thêm hình dán ngôi sao?",
            QuestionType = MediaType.Switch,
            IsInformationRequired = false,
            IsQuantity = true
        },
        new()
        {
            Id = Guid.Parse("07d87d20-3176-497a-b665-2932f5e7d7d4"),
            Name = "Inside flap",
            Question = "",
            QuestionType = MediaType.Switch,
            IsInformationRequired = false,
            IsQuantity = false
        },
#endregion
    };

    private static readonly List<PackageFeature> PackageFeatures = new()
    {
        #region Sample PackageFeature of service Làm dây chuyền chữ Y (dùng để test)
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
        #endregion

        #region PackageFeature of package Basic, service: "Wallet for Valentine Day"
        new()
        {
            Id = Guid.Parse("7cdb3bd2-259a-445d-b521-6b8781adc277"),
            Name = "Rustic Brown",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[3].Id,
            FeatureId = Features[4].Id
        },
        new()
        {
            Id = Guid.Parse("50da6db7-f16a-4409-ba94-7e9194bb9204"),
            Name = "Brown Oily",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[3].Id,
            FeatureId = Features[4].Id
        },
        new()
        {
            Id = Guid.Parse("4e078d74-5d79-4066-802a-87dfbf163ffe"),
            Name = "Mini (38 mm x 64 mm)",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[3].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("179bf376-c57c-4037-aac6-773b109cbaa6"),
            Name = "Standard (64 mm x 89 mm)",
            IsExtra = true,
            AdditionalCost = 10000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[3].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("071908d8-2dcd-4bb0-a986-4db68d937890"),
            Name = "Large (51 mm x 76 mm)",
            IsExtra = true,
            AdditionalCost = 20000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[3].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("5b05f3e6-1565-4a42-b515-8b5fff359546"),
            Name = "5",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 5,
            PackageId = Packages[3].Id,
            FeatureId = Features[8].Id
        },
        new()
        {
            Id = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
            Name = "Hình dán",
            IsExtra = true,
            AdditionalCost = 1000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 10,
            PackageId = Packages[3].Id,
            FeatureId = Features[10].Id
        },
        #endregion

        #region PackageFeature of package Standard, service: "Wallet for Valentine Day"
        new()
        {
            Id = Guid.Parse("5d44a142-80af-4054-8e7d-ef440181dda3"),
            Name = "Rustic Brown",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[4].Id
        },
        new()
        {
            Id = Guid.Parse("947d3c1d-2e29-45ea-a965-d6dbc9d5a503"),
            Name = "Brown Oily",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[4].Id
        },
        new()
        {
            Id = Guid.Parse("8b6e33a8-1def-423a-a6c8-9306245d33e8"),
            Name = "Mini (38 mm x 64 mm)",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("d4ed5890-9abd-453a-8adc-e43148864ee9"),
            Name = "Standard (64 mm x 89 mm)",
            IsExtra = true,
            AdditionalCost = 10000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("5e34916e-8cca-45a2-b922-c98db668549f"),
            Name = "Large (51 mm x 76 mm)",
            IsExtra = true,
            AdditionalCost = 20000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("03a5408f-56b0-4a24-9884-ae32bb4b33a0"),
            Name = "Thẻ tên",
            IsExtra = true,
            AdditionalCost = 20000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[6].Id
        },
        new()
        {
            Id = Guid.Parse("7663c879-e53a-4cf7-9314-5a56c10cf997"),
            Name = "Chuông",
            IsExtra = true,
            AdditionalCost = 5000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[6].Id
        },
        new()
        {
            Id = Guid.Parse("46337b72-992b-4e80-9828-d07386afb141"),
            Name = "Dây xích",
            IsExtra = true,
            AdditionalCost = 7000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[6].Id
        },
        new()
        {
            Id = Guid.Parse("a29ab9b9-2d03-4375-ab68-f5a395d591e5"),
            Name = "True",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[7].Id
        },
        new()
        {
            Id = Guid.Parse("02ecd8e3-8410-43d9-bdc5-3701dfa2109d"),
            Name = "10",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 10,
            PackageId = Packages[4].Id,
            FeatureId = Features[8].Id
        },
        new()
        {
            Id = Guid.Parse("a2690141-4af6-4a45-884a-9b7c71bb4e7b"),
            Name = "True",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[4].Id,
            FeatureId = Features[9].Id
        },
        new()
        {
            Id = Guid.Parse("a1a6fdd8-c19a-43c4-99a9-6c6b424e9e36"),
            Name = "Hình dán",
            IsExtra = true,
            AdditionalCost = 1000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 10,
            PackageId = Packages[4].Id,
            FeatureId = Features[10].Id
        },
        #endregion

        #region PackageFeature of package Premium, service: "Wallet for Valentine Day"
        new()
        {
            Id = Guid.Parse("42e781b4-83fa-46c6-a8d2-aba9b9468b8a"),
            Name = "Rustic Brown",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[4].Id
        },
        new()
        {
            Id = Guid.Parse("c14463bc-d755-4f65-8043-a1c005734133"),
            Name = "Brown Oily",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[4].Id
        },
        new()
        {
            Id = Guid.Parse("416e224c-1119-482c-b8f6-503ca5eee81a"),
            Name = "Mini (38 mm x 64 mm)",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("a79645f4-be8e-4c97-9f62-355536340704"),
            Name = "Standard (64 mm x 89 mm)",
            IsExtra = true,
            AdditionalCost = 10000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("b9ed3f96-f127-4bd6-b310-e3493136c296"),
            Name = "Large (51 mm x 76 mm)",
            IsExtra = true,
            AdditionalCost = 20000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[5].Id
        },
        new()
        {
            Id = Guid.Parse("475aa523-b039-459b-916a-65896ac29e9b"),
            Name = "Thẻ tên",
            IsExtra = true,
            AdditionalCost = 20000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[6].Id
        },
        new()
        {
            Id = Guid.Parse("1acf0999-c20c-4192-814a-fdcf6aae7260"),
            Name = "Chuông",
            IsExtra = true,
            AdditionalCost = 5000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[6].Id
        },
        new()
        {
            Id = Guid.Parse("7f1b07a4-2eff-44f8-90dd-3e64786beea6"),
            Name = "Dây xích",
            IsExtra = true,
            AdditionalCost = 7000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[6].Id
        },
        new()
        {
            Id = Guid.Parse("6ef5683b-935f-4599-8011-5cd45f8de5e0"),
            Name = "True",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[7].Id
        },
        new()
        {
            Id = Guid.Parse("6b03623a-d59d-4d1a-816f-742e297ef240"),
            Name = "12",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 10,
            PackageId = Packages[5].Id,
            FeatureId = Features[8].Id
        },
        new()
        {
            Id = Guid.Parse("f50c24f8-10ed-46fa-80df-20ba0aa0e61d"),
            Name = "True",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[11].Id
        },
        new()
        {
            Id = Guid.Parse("4eba12bb-164b-45b5-a3cc-334496551c8b"),
            Name = "True",
            IsExtra = false,
            AdditionalCost = 0,
            AdditionalDay = null,
            IsChecked = true,
            MaxQuantity = 0,
            PackageId = Packages[5].Id,
            FeatureId = Features[9].Id
        },
        new()
        {
            Id = Guid.Parse("f4b887d3-9730-44ec-b774-26f77a42c347"),
            Name = "Hình dán",
            IsExtra = true,
            AdditionalCost = 1000,
            AdditionalDay = null,
            IsChecked = false,
            MaxQuantity = 10,
            PackageId = Packages[5].Id,
            FeatureId = Features[10].Id
        },
        #endregion
    };
    private static readonly List<CancellationReason> CancellationReasons = new()
    {
        new()
        {
            Id = Guid.Parse("abca3b34-d33e-479b-8940-da24f2a5bacd"),
            Name = "A",
            Value = 1
        }
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
        OriginPrice = 2500000,
        Quantity = 2,
        ShipmentCode = "ORDCHD125FG8G1_Delivery",
        CancleOrderReason = CancleOrderReason.ChangeShipmentAddress,
        DeliveryTime = DateTime.UtcNow.AddDays(7),
        CurrentSketchRevision = 1,
        Stage = OrderStage.Shipping,
        Status = OrderStatus.Pending,
        TotalPrice = 2500000,
        AfterApplyVoucherPrice = 2500000,
        AdminCommDefault = 200000,
        AdminCommUsedVch = null,
        ArtistRevenue = 2000000,
        VoucherCost = 0,
        PackageId = Packages[0].Id,
        CreatedById = Accounts[0].Id,
        CancellationReasonId = CancellationReasons[0].Id,
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
        OriginPrice = 2500000,
        Quantity = 1,
        ShipmentCode = "ORDCHD125FG8G1_Return",
        CancleOrderReason = CancleOrderReason.ChangeShipmentAddress,
        DeliveryTime = DateTime.UtcNow.AddDays(7),
        CurrentSketchRevision = 1,
        Stage = OrderStage.Shipping,
        Status = OrderStatus.Pending,
        TotalPrice = 2500000,
        AfterApplyVoucherPrice = 2500000,
        AdminCommDefault = 200000,
        AdminCommUsedVch = null,
        ArtistRevenue = 2000000,
        VoucherCost = 0,
        PackageId = Packages[0].Id,
        CreatedById = Accounts[0].Id,
        CancellationReasonId = CancellationReasons[0].Id,
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

        foreach (var cancellationReason in CancellationReasons)
        {
            if (!context.CancellationReasons.Any(i => i.Id == cancellationReason.Id))
            {
                cancellationReason.CreationDate = DateTime.UtcNow;
                context.CancellationReasons.Add(cancellationReason);
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
