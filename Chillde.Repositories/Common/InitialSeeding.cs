using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Microsoft.EntityFrameworkCore;
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
        new() { Id = Guid.Parse("0195a73a-98fe-71a2-a007-25cc6c3d26f1"), Name = Enums.Role.Admin.ToString() },
        new() { Id = Guid.Parse("0195a73a-9919-714b-a9ad-29b849e946f7"), Name = Enums.Role.Customer.ToString() },
        new() { Id = Guid.Parse("0195a73a-991d-70f0-aca4-78e00ff7557b"), Name = Enums.Role.Artisan.ToString() }
    };

    private static readonly List<Wallet> Wallets = new()
    {
        new()
        {
            Id = Guid.Parse("f335111a-1b3c-485f-aa87-ebbf689dbd4c"),
            Balance = 1000000000000,
            CreatedById = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b")
        },
        new Wallet()
        {
            Id = Guid.Parse("01958fac-8839-70e4-86b5-46e245936b42"),
            Balance = 500000,
        }
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
        },
        new Account()
        {
            Id = Guid.Parse("01958fac-878c-7a59-b2b9-ea2521f3a1ff"),
            FirstName = "Văn Hải",
            LastName = "Đặng",
            Username = "vanhaitsu",
            Email = "vanhaicntt5@gmail.com",
            HashedPassword = "$2a$11$rL..OUglbn/AAQlXarix2u9EsRk36vx2NN.0trthImOyJcK7K6iia",
            Image =
                "https://res.cloudinary.com/dhktjmuv6/image/upload/v1741952342/01958fac-878c-7a59-b2b9-ea2521f3a1ff_image.jpg",
            StoreAddress = "Thành phố Hồ Chí Minh",
            Banner =
                "https://res.cloudinary.com/dhktjmuv6/image/upload/v1741952349/01958fac-878c-7a59-b2b9-ea2521f3a1ff_banner.jpg",
            StoreDescription =
                "Chào mừng đến với không gian sáng tạo nơi những món quà độc đáo và ý nghĩa được sinh ra. Chúng tôi chuyên cung cấp dịch vụ làm đồ handmade, mang đến cho bạn những sản phẩm thủ công tinh tế và đầy cá tính. Từ những chiếc vòng cổ xinh xắn đến những chiếc túi tote độc đáo, mỗi món đồ đều được làm bằng tình yêu và sự tỉ mỉ. Hãy để chúng tôi giúp bạn tạo ra những món quà đặc biệt cho bản thân hoặc những người thân yêu. Liên hệ với chúng tôi ngay hôm nay để khám phá thế giới thủ công đầy màu sắc và ý nghĩa!",
            EmailConfirmed = true,
            PhoneNumberConfirmed = false,
            Status = AccountStatus.Active,
            WalletId = Guid.Parse("01958fac-8839-70e4-86b5-46e245936b42")
        }
    };

    private static readonly List<AccountRole> AccountRoles = new()
    {
        // vanhaitsu
        // Customer
        new()
        {
            AccountId = Guid.Parse("01958fac-878c-7a59-b2b9-ea2521f3a1ff"),
            RoleId = Guid.Parse("0195a73a-9919-714b-a9ad-29b849e946f7")
        },
        // Artisan
        new()
        {
            AccountId = Guid.Parse("01958fac-878c-7a59-b2b9-ea2521f3a1ff"),
            RoleId = Guid.Parse("0195a73a-991d-70f0-aca4-78e00ff7557b")
        }
    };

    private static readonly List<Category> Categories = new()
    {
         // Dữ liệu cũ được giữ nguyên
        new()
    {
        Id = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"),
        Name = "Accessories",
        AttachmentUrl =
            "https://images.unsplash.com/photo-1569388330338-53ecda03dfa1?q=80&w=2670&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        Name = "Art & Collectibles",
        AttachmentUrl =
            "https://images.unsplash.com/photo-1695142258314-180ea86bdb48?q=80&w=2670&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("b698f941-943c-47d1-88fd-9e0e05f6e15a"),
        Name = "Jewelry",
        AttachmentUrl =
            "https://images.unsplash.com/photo-1543294001-f7cd5d7fb516?q=80&w=2670&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("f2ef8608-50f5-48e5-a6a3-51e2dbdf065b"),
        Name = "Belts & Suspenders",
        ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"),
        AttachmentUrl =
            "https://images.unsplash.com/photo-1664286022075-8e997e95bd17?q=80&w=2670&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("85a007bc-b215-46fe-98bc-eac1d44e2234"),
        Name = "Sunglasses & Eyewear",
        ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"),
        AttachmentUrl =
            "https://images.unsplash.com/photo-1511499767150-a48a237f0083?q=80&w=2680&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("f576216f-b84c-4b71-adff-0d67f21ae7a3"),
        Name = "Bouquets & Corsages",
        ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"),
        AttachmentUrl =
            "https://images.unsplash.com/photo-1668233342581-ce94a57d9e01?q=80&w=2663&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("723b7ac6-ea6f-4ba0-b456-2bd6ea225e3e"),
        Name = "Prints",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl =
            "https://images.unsplash.com/photo-1626868554387-ec8e6e1aef18?q=80&w=2574&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("ba815129-7ad6-4d2d-ba7b-a43977bc310e"),
        Name = "Earrings",
        ParentId = Guid.Parse("b698f941-943c-47d1-88fd-9e0e05f6e15a"),
        AttachmentUrl =
            "https://images.unsplash.com/photo-1629224316810-9d8805b95e76?q=80&w=2670&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Dữ liệu mới từ Etsy Accessories (sử dụng GUID mới)
    // Level 1: Baby Accessories
    new()
    {
        Id = Guid.Parse("e51ae29b-e048-4db0-89a2-670a44e6eea7"),
        Name = "Baby Accessories",
        ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"),
        AttachmentUrl = "https://images.unsplash.com/photo-1515488042361-ee00e0ddd4e4?q=80&w=2075&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" // Placeholder
    },
    new()
    {
        Id = Guid.Parse("f335111a-1b3c-485f-aa87-ebbf689dbd4c"),
        Name = "Baby Carriers & Wraps",
        ParentId = Guid.Parse("e51ae29b-e048-4db0-89a2-670a44e6eea7"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1663133440874-586ef3c834e1?q=80&w=2098&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("0b95fbfe-b958-47ef-9672-3dd0131fa13b"),
        Name = "Children's Photo Props",
        ParentId = Guid.Parse("e51ae29b-e048-4db0-89a2-670a44e6eea7"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1696408290876-4c8b13a921c4?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Belts & Suspenders (đã có trong dữ liệu cũ, thêm subcategories mới nếu cần)
    new()
    {
        Id = Guid.Parse("6551bec5-12eb-49e1-ba72-134545db85dc"),
        Name = "Belt Buckles",
        ParentId = Guid.Parse("f2ef8608-50f5-48e5-a6a3-51e2dbdf065b"),
        AttachmentUrl = "https://images.unsplash.com/photo-1608461864721-b8f50c91c147?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("8bb06e3f-17da-47d8-b6f3-44e6325921cc"),
        Name = "Belts",
        ParentId = Guid.Parse("f2ef8608-50f5-48e5-a6a3-51e2dbdf065b"),
        AttachmentUrl = "https://images.unsplash.com/photo-1591117105338-4eb266b13c7d?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("d2ddc903-b467-4867-acf5-ab5a5f187288"),
        Name = "Suspenders",
        ParentId = Guid.Parse("f2ef8608-50f5-48e5-a6a3-51e2dbdf065b"),
        AttachmentUrl = "https://images.unsplash.com/photo-1618001789196-8b986847cd5e?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Bouquets & Corsages (đã có, thêm subcategories)
    new()
    {
        Id = Guid.Parse("220dcbe8-8388-45e9-9a8e-c09fc246abff"),
        Name = "Bouquets",
        ParentId = Guid.Parse("f576216f-b84c-4b71-adff-0d67f21ae7a3"),
        AttachmentUrl = "https://images.unsplash.com/photo-1523694576729-dc99e9c0f9b4?q=80&w=1935&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("3ee74bb9-f578-42cd-b214-eca534f3a89b"),
        Name = "Boutonnières",
        ParentId = Guid.Parse("f576216f-b84c-4b71-adff-0d67f21ae7a3"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1672883552124-5353ca268f32?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("811219b6-29e6-46e0-96d3-eba76d334895"),
        Name = "Corsages",
        ParentId = Guid.Parse("f576216f-b84c-4b71-adff-0d67f21ae7a3"),
        AttachmentUrl = "https://images.unsplash.com/photo-1604946388121-031fb0dc4def?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Costume Accessories
    new()
    {
        Id = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        Name = "Costume Accessories",
        ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1675186049530-33e9479f0298?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("fcfedfc2-3fbc-41c3-94ad-e922bdbe88e6"),
        Name = "Capes",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://images.unsplash.com/photo-1635805251780-7cb3a032d906?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("07d87d20-3176-497a-b665-2932f5e7d7d4"),
        Name = "Costume Goggles",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://images.unsplash.com/photo-1512484580809-b5251c5df9dd?q=80&w=1977&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("9117fe29-9fec-4cf5-8f52-37d1efec7b22"),
        Name = "Costume Hats & Headpieces",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://images.unsplash.com/photo-1542000551557-3fd0ad0eb15f?q=80&w=2071&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("fbfd784f-bfe3-49ed-99b5-19c46c19b6f1"),
        Name = "Costume Tails & Ears",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://images.unsplash.com/photo-1572580480606-5cec850f9666?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("4871b8c7-af47-4bb6-8fad-226fb5e84d2e"),
        Name = "Costume Ears",
        ParentId = Guid.Parse("fbfd784f-bfe3-49ed-99b5-19c46c19b6f1"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1723867346889-b3e4b1de84a0?q=80&w=2006&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a03772d4-9c79-406c-8b1a-6c586df22b15"),
        Name = "Costume Tails",
        ParentId = Guid.Parse("fbfd784f-bfe3-49ed-99b5-19c46c19b6f1"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1693679488761-57f029a69cfb?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("6c8d8723-d2b1-48fd-8fea-46d3b70782e1"),
        Name = "Costume Weapons",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://images.unsplash.com/photo-1697715841367-62fbb3fe1683?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("7b218041-fe35-4c61-a6c0-83d887bbb67e"),
        Name = "Facial Hair",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://images.unsplash.com/photo-1591082735306-ee50d6501185?q=80&w=1964&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("7cdb3bd2-259a-445d-b521-6b8781adc277"),
        Name = "Halloween Candy Bags",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://images.unsplash.com/photo-1506805259407-9569fa21ccd1?q=80&w=1935&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("50da6db7-f16a-4409-ba94-7e9194bb9204"),
        Name = "Masks & Prosthetics",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://images.unsplash.com/photo-1602541975176-00defcf886ce?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("4e078d74-5d79-4066-802a-87dfbf163ffe"),
        Name = "Masks",
        ParentId = Guid.Parse("50da6db7-f16a-4409-ba94-7e9194bb9204"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1677354136477-6c9f75f65038?q=80&w=1976&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("179bf376-c57c-4037-aac6-773b109cbaa6"),
        Name = "Prosthetics",
        ParentId = Guid.Parse("50da6db7-f16a-4409-ba94-7e9194bb9204"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1682096969085-3eaa044fe3c1?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("071908d8-2dcd-4bb0-a986-4db68d937890"),
        Name = "Wands",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1665949503036-786df9fbc7b8?q=80&w=1976&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("5b05f3e6-1565-4a42-b515-8b5fff359546"),
        Name = "Wings",
        ParentId = Guid.Parse("59a5fa0a-9cb9-44c5-95e5-b55395584e89"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1683930387499-f432c92ab4d8?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Gloves & Mittens
    new()
    {
        Id = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        Name = "Gloves & Mittens",
        ParentId = Guid.Parse("da08e1e1-7f54-4e7a-956c-80bf0632013b"),
        AttachmentUrl = "https://images.unsplash.com/photo-1452689842785-5f14840dca48?q=80&w=1728&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("5d44a142-80af-4054-8e7d-ef440181dda3"),
        Name = "Arm Warmers",
        ParentId = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1695604460925-4fc8f2722362?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("947d3c1d-2e29-45ea-a965-d6dbc9d5a503"),
        Name = "Costume Gloves",
        ParentId = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1677110758452-70a02d403bbf?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("8b6e33a8-1def-423a-a6c8-9306245d33e8"),
        Name = "Driving Gloves",
        ParentId = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        AttachmentUrl = "https://images.unsplash.com/photo-1516579486067-6d7ef4d67c1e?q=80&w=2071&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("d4ed5890-9abd-453a-8adc-e43148864ee9"),
        Name = "Evening & Formal Gloves",
        ParentId = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1678457828662-f9cb76b76ea1?q=80&w=1976&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("5e34916e-8cca-45a2-b922-c98db668549f"),
        Name = "Gardening & Work Gloves",
        ParentId = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1679446269688-3a2576ca02c9?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("03a5408f-56b0-4a24-9884-ae32bb4b33a0"),
        Name = "Gardening Gloves",
        ParentId = Guid.Parse("5e34916e-8cca-45a2-b922-c98db668549f"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1678723981909-b3a9f3f3c34d?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("7663c879-e53a-4cf7-9314-5a56c10cf997"),
        Name = "Work Gloves",
        ParentId = Guid.Parse("5e34916e-8cca-45a2-b922-c98db668549f"),
        AttachmentUrl = "https://images.unsplash.com/photo-1484999691661-51b16a51d1d1?q=80&w=2104&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("46337b72-992b-4e80-9828-d07386afb141"),
        Name = "Mittens & Muffs",
        ParentId = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        AttachmentUrl = "https://images.unsplash.com/photo-1613662311577-f544f7969f5a?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a29ab9b9-2d03-4375-ab68-f5a395d591e5"),
        Name = "Mittens",
        ParentId = Guid.Parse("46337b72-992b-4e80-9828-d07386afb141"),
        AttachmentUrl = "https://images.unsplash.com/photo-1452689842785-5f14840dca48?q=80&w=1728&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("02ecd8e3-8410-43d9-bdc5-3701dfa2109d"),
        Name = "Muffs",
        ParentId = Guid.Parse("46337b72-992b-4e80-9828-d07386afb141"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1672078159323-4d09f719d3af?q=80&w=1972&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a2690141-4af6-4a45-884a-9b7c71bb4e7b"),
        Name = "Sports Gloves",
        ParentId = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        AttachmentUrl = "https://images.unsplash.com/photo-1723910705316-ca7e874cc681?q=80&w=2155&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a1a6fdd8-c19a-43c4-99a9-6c6b424e9e36"),
        Name = "Winter Gloves",
        ParentId = Guid.Parse("92c60a43-88d5-48e6-bc24-4e87a12dec37"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1671003995983-e72fabf7f183?q=80&w=1976&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c1e2f3d4-5678-9abc-def0-1234567890ab"),
        Name = "Clothing",
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1679056835084-7f21e64a3402?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    
    // Level 1: Boys' Clothing
    new()
    {
        Id = Guid.Parse("c2e3f4d5-6789-abcd-ef01-234567890abc"),
        Name = "Boys' Clothing",
        ParentId = Guid.Parse("c1e2f3d4-5678-9abc-def0-1234567890ab"),
        AttachmentUrl = "https://images.unsplash.com/photo-1641708594063-46b5bbe83e40?q=80&w=1935&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c3e4f5d6-789a-bcde-f012-34567890abcd"),
        Name = "Baby Boys' Clothing",
        ParentId = Guid.Parse("c2e3f4d5-6789-abcd-ef01-234567890abc"),
        AttachmentUrl = "https://images.unsplash.com/photo-1641708594063-46b5bbe83e40?q=80&w=1935&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c4e5f6d7-89ab-cdef-0123-4567890abcde"),
        Name = "Bloomers, Diaper Covers & Underwear",
        ParentId = Guid.Parse("c3e4f5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://images.unsplash.com/photo-1592318324785-c41691d2e773?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c5e6f7d8-9abc-def0-1234-567890abcdef"),
        Name = "Bloomers",
        ParentId = Guid.Parse("c4e5f6d7-89ab-cdef-0123-4567890abcde"),
        AttachmentUrl = "https://images.unsplash.com/photo-1577746838292-8c224f0b7350?q=80&w=1983&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c6e7f8d9-abcd-ef01-2345-67890abcdef0"),
        Name = "Diaper Covers",
        ParentId = Guid.Parse("c4e5f6d7-89ab-cdef-0123-4567890abcde"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1723563578392-d38fed807512?q=80&w=1956&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c7e8f9da-bcde-f012-3456-7890abcdef01"),
        Name = "Underwear",
        ParentId = Guid.Parse("c4e5f6d7-89ab-cdef-0123-4567890abcde"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1671717726282-7f2f479f0c87?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c8e9fadb-cdef-0123-4567-890abcdef012"),
        Name = "Bodysuits",
        ParentId = Guid.Parse("c3e4f5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1671641797833-f0b00d133830?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c9eafbcd-def0-1234-5678-90abcdef0123"),
        Name = "Clothing Sets",
        ParentId = Guid.Parse("c3e4f5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1723553201287-ab9a8fbe57d1?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("cafbcdde-ef01-2345-6789-0abcdef01234"),
        Name = "Costumes",
        ParentId = Guid.Parse("c3e4f5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://images.unsplash.com/photo-1565249090594-eb2283642af2?q=80&w=1976&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("cbcddeef-f012-3456-7890-abcdef012345"),
        Name = "Hoodies & Sweatshirts",
        ParentId = Guid.Parse("c3e4f5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://images.unsplash.com/photo-1621027212913-da785ebe2bcb?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("ccddeef0-0123-4567-890a-bcdef0123456"),
        Name = "Hoodies",
        ParentId = Guid.Parse("cbcddeef-f012-3456-7890-abcdef012345"),
        AttachmentUrl = "https://images.unsplash.com/photo-1622567893612-a5345baa5c9a?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("cddeef01-1234-5678-90ab-cdef01234567"),
        Name = "Sweatshirts",
        ParentId = Guid.Parse("cbcddeef-f012-3456-7890-abcdef012345"),
        AttachmentUrl = "https://images.unsplash.com/photo-1614975059251-992f11792b9f?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("cdeef012-2345-6789-0abc-def012345678"),
        Name = "Pants",
        ParentId = Guid.Parse("c3e4f5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://images.unsplash.com/photo-1541099649105-f69ad21f3246?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("cef01234-3456-7890-abcd-ef0123456789"),
        Name = "Socks & Leg Warmers",
        ParentId = Guid.Parse("c3e4f5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://images.unsplash.com/photo-1454720503269-3a35c21bebc6?q=80&w=2012&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("cf012345-4567-890a-bcde-f01234567890"),
        Name = "Leg Warmers",
        ParentId = Guid.Parse("cef01234-3456-7890-abcd-ef0123456789"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1729188135022-9f4c75716298?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("d0123456-5678-90ab-cdef-0123456789ab"),
        Name = "Socks",
        ParentId = Guid.Parse("cef01234-3456-7890-abcd-ef0123456789"),
        AttachmentUrl = "https://images.unsplash.com/photo-1585499583264-491df5142e83?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Gender-Neutral Adult Clothing
    new()
    {
        Id = Guid.Parse("d1234567-6789-0abc-def0-123456789abc"),
        Name = "Gender-Neutral Adult Clothing",
        ParentId = Guid.Parse("c1e2f3d4-5678-9abc-def0-1234567890ab"),
        AttachmentUrl = "https://images.unsplash.com/photo-1525507119028-ed4c629a60a3?q=80&w=1935&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("d2345678-7890-abcd-ef01-234567890abc"),
        Name = "Blazers",
        ParentId = Guid.Parse("d1234567-6789-0abc-def0-123456789abc"),
        AttachmentUrl = "https://images.unsplash.com/photo-1592878904946-b3cd8ae243d0?q=80&w=2081&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("d7890abc-cdef-0123-4567-890abcdef012"),
        Name = "Jackets & Coats",
        ParentId = Guid.Parse("d1234567-6789-0abc-def0-123456789abc"),
        AttachmentUrl = "https://images.unsplash.com/photo-1616150840617-a0124ea42a1f?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("d890abcd-def0-1234-5678-90abcdef0123"),
        Name = "Jeans",
        ParentId = Guid.Parse("d1234567-6789-0abc-def0-123456789abc"),
        AttachmentUrl = "https://images.unsplash.com/photo-1604176354204-9268737828e4?q=80&w=2080&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("d90abcde-ef01-2345-6789-0abcdef01234"),
        Name = "Leggings",
        ParentId = Guid.Parse("d1234567-6789-0abc-def0-123456789abc"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1665664652383-2308d742943c?q=80&w=1964&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("dabcdeef-f012-3456-7890-abcdef012345"),
        Name = "Overalls & Coveralls",
        ParentId = Guid.Parse("d1234567-6789-0abc-def0-123456789abc"),
        AttachmentUrl = "https://images.unsplash.com/photo-1586973644827-9f7cbacd0b85?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    // Level 1: Artist Trading Cards
    new()
    {
        Id = Guid.Parse("a2b3c4d5-6789-abcd-ef01-234567890abc"),
        Name = "Artist Trading Cards",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1606355555437-59e0b3f0a703?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Collectibles
    new()
    {
        Id = Guid.Parse("a3b4c5d6-789a-bcde-f012-34567890abcd"),
        Name = "Collectibles",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1590708622734-b1b8df3c3576?q=80&w=1935&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a4b5c6d7-89ab-cdef-0123-4567890abcde"),
        Name = "Advertisements",
        ParentId = Guid.Parse("a3b4c5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://images.unsplash.com/photo-1579677917230-8a938ffc0279?q=80&w=1936&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a5b6c7d8-9abc-def0-1234-567890abcdef"),
        Name = "Coins & Money",
        ParentId = Guid.Parse("a3b4c5d6-789a-bcde-f012-34567890abcd"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1671803954021-a203ec6d675a?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    // Level 1: Dolls & Miniatures
    new()
    {
        Id = Guid.Parse("a6b7c8d9-abcd-ef01-2345-67890abcdef0"),
        Name = "Dolls & Miniatures",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1608061084182-ad960c59a66a?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a7b8c9da-bcde-f012-3456-7890abcdef01"),
        Name = "Art Dolls",
        ParentId = Guid.Parse("a6b7c8d9-abcd-ef01-2345-67890abcdef0"),
        AttachmentUrl = "https://images.unsplash.com/photo-1580136607986-df0f19794f41?q=80&w=1984&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a8b9cdab-cdef-0123-4567-890abcdef012"),
        Name = "Goth & Horror Dolls",
        ParentId = Guid.Parse("a6b7c8d9-abcd-ef01-2345-67890abcdef0"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1698279879712-0dbd1df2dd5a?q=80&w=2002&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("a9bcdabc-def0-1234-5678-90abcdef0123"),
        Name = "Dioramas",
        ParentId = Guid.Parse("a6b7c8d9-abcd-ef01-2345-67890abcdef0"),
        AttachmentUrl = "https://images.unsplash.com/photo-1708523404126-2f93c4033cda?q=80&w=1935&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("abcdabcd-ef01-2345-6789-0abcdef01234"),
        Name = "Dollhouses",
        ParentId = Guid.Parse("a6b7c8d9-abcd-ef01-2345-67890abcdef0"),
        AttachmentUrl = "https://images.unsplash.com/photo-1608701570436-30a168ecb4b6?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    // Level 1: Drawing & Illustration
    new()
    {
        Id = Guid.Parse("b1c2d3e4-5678-9abc-def0-1234567890ab"),
        Name = "Drawing & Illustration",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1575995872537-3793d29d972c?q=80&w=1924&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("b2c3d4e5-6789-abcd-ef01-234567890abc"),
        Name = "Architectural Drawings",
        ParentId = Guid.Parse("b1c2d3e4-5678-9abc-def0-1234567890ab"),
        AttachmentUrl = "https://images.unsplash.com/photo-1599420186985-5c3d1a038e84?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("b3c4d5e6-789a-bcde-f012-34567890abcd"),
        Name = "Charcoal",
        ParentId = Guid.Parse("b1c2d3e4-5678-9abc-def0-1234567890ab"),
        AttachmentUrl = "https://images.unsplash.com/photo-1575025823481-4d4a0a72d656?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Fiber Arts
    new()
    {
        Id = Guid.Parse("b4c5d6e7-89ab-cdef-0123-4567890abcde"),
        Name = "Fiber Arts",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1669844483981-42471e3ec732?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("b5c6d7e8-9abc-def0-1234-567890abcdef"),
        Name = "Batik",
        ParentId = Guid.Parse("b4c5d6e7-89ab-cdef-0123-4567890abcde"),
        AttachmentUrl = "https://images.unsplash.com/photo-1604973104381-870c92f10343?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("b6c7d8e9-abcd-ef01-2345-67890abcdef0"),
        Name = "Crewel",
        ParentId = Guid.Parse("b4c5d6e7-89ab-cdef-0123-4567890abcde"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1692650759832-4d674420e5e7?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("b7c8d9ea-bcde-f012-3456-7890abcdef01"),
        Name = "Crochet",
        ParentId = Guid.Parse("b4c5d6e7-89ab-cdef-0123-4567890abcde"),
        AttachmentUrl = "https://images.unsplash.com/photo-1519412849983-957822373d02?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    // Level 1: Fine Art Ceramics
    new()
    {
        Id = Guid.Parse("b8c9deab-cdef-0123-4567-890abcdef012"),
        Name = "Fine Art Ceramics",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1682147026652-d4ea9fb76efd?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Glass Art
    new()
    {
        Id = Guid.Parse("b9cdeabc-def0-1234-5678-90abcdef0123"),
        Name = "Glass Art",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1619824130478-2fb945b98ae1?q=80&w=1939&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("bcdeabcd-ef01-2345-6789-0abcdef01234"),
        Name = "Glass Sculptures & Figurines",
        ParentId = Guid.Parse("b9cdeabc-def0-1234-5678-90abcdef0123"),
        AttachmentUrl = "https://images.unsplash.com/photo-1466027785809-90d27831b9bd?q=80&w=2072&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Mosaics
    new()
    {
        Id = Guid.Parse("c1d2e3f4-5678-9abc-def0-1234567890ab"),
        Name = "Mosaics",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1595916430766-95b932a5b032?q=80&w=1935&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Painting
    new()
    {
        Id = Guid.Parse("c2d3e4f5-6789-abcd-ef01-234567890abc"),
        Name = "Painting",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1579965342575-16428a7c8881?q=80&w=1962&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c3d4e5f6-789a-bcde-f012-34567890abcd"),
        Name = "Acrylic",
        ParentId = Guid.Parse("c2d3e4f5-6789-abcd-ef01-234567890abc"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1672329273339-350b22ab1f95?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c4d5e6f7-89ab-cdef-0123-4567890abcde"),
        Name = "Combination",
        ParentId = Guid.Parse("c2d3e4f5-6789-abcd-ef01-234567890abc"),
        AttachmentUrl = "https://images.unsplash.com/photo-1573706518886-f90f89c72f14?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    // Level 1: Photography
    new()
    {
        Id = Guid.Parse("c5d6e7f8-9abc-def0-1234-567890abcdef"),
        Name = "Photography",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1554048612-b6a482bc67e5?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c6d7e8f9-abcd-ef01-2345-67890abcdef0"),
        Name = "Black & White",
        ParentId = Guid.Parse("c5d6e7f8-9abc-def0-1234-567890abcdef"),
        AttachmentUrl = "https://images.unsplash.com/photo-1548516173-3cabfa4607e9?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c7d8e9fa-bcde-f012-3456-7890abcdef01"),
        Name = "Color",
        ParentId = Guid.Parse("c5d6e7f8-9abc-def0-1234-567890abcdef"),
        AttachmentUrl = "https://images.unsplash.com/photo-1463438690606-f6778b8c1d10?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    // Level 1: Bags & Purses
    new()
    {
        Id = Guid.Parse("c8d9efab-cdef-0123-4567-890abcdef012"),
        Name = "Bags & Purses",
        ParentId = Guid.Parse("1ac6acb2-dadc-4882-8561-1716ef2d5c52"),
        AttachmentUrl = "https://images.unsplash.com/photo-1524672353063-4f66ee1f385e?q=80&w=2069&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("c9defabc-def0-1234-5678-90abcdef0123"),
        Name = "Accessory Cases",
        ParentId = Guid.Parse("c8d9efab-cdef-0123-4567-890abcdef012"),
        AttachmentUrl = "https://images.unsplash.com/photo-1577954732026-2071521acdfb?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },
    new()
    {
        Id = Guid.Parse("cdefabcd-ef01-2345-6789-0abcdef01234"),
        Name = "Cigarette Cases",
        ParentId = Guid.Parse("c8d9efab-cdef-0123-4567-890abcdef012"),
        AttachmentUrl = "https://plus.unsplash.com/premium_photo-1664392091988-dfc50fac7ab6?q=80&w=1962&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    },

    };

    //private static readonly List<C> SubCategories = new()
    //{

    //        new() { Id = Guid.Parse("19001a42-5d0b-49f1-be6a-d975145d2c48"), Code = "SC05", Name = "Sculpture", CategoryId = Categories[1].Id},
    //        new() { Id = Guid.Parse("e90ad4b0-2c62-4614-9a8f-307371d8420e"), Code = "SC06", Name = "Painting", CategoryId = Categories[1].Id},
    //        new() { Id = Guid.Parse("f48bec9d-c653-4952-973a-72c20e07341d"), Code = "SC07", Name = "Necklaces", CategoryId = Categories[2].Id},

    //        new() { Id = Guid.Parse("ec15a653-a1d9-4926-b8b5-79b556350928"), Code = "SC09", Name = "Rings", CategoryId = Categories[2].Id},
    //};
    private static readonly List<SystemConfig> SystemConfigs = new()
    {
        new()
        {
            EntityType = ConfigType.Security, FieldName = "AccessTokenValidityInMinutes",
            Value = JsonDocument.Parse("5")
        },
        new()
        {
            EntityType = ConfigType.Security, FieldName = "RefreshTokenValidityInDays", Value = JsonDocument.Parse("7")
        },
        new()
        {
            EntityType = ConfigType.Security, FieldName = "VerificationCodeValidityInMinutes",
            Value = JsonDocument.Parse("15")
        },
        new()
        {
            EntityType = ConfigType.Security, FieldName = "VerificationCodeLength", Value = JsonDocument.Parse("6")
        },
        new()
        {
            EntityType = ConfigType.Security, FieldName = "ResetPasswordTokenValidityInMinutes",
            Value = JsonDocument.Parse("15")
        },
        new()
        {
            EntityType = ConfigType.Pagination, FieldName = "DefaultMinPageSize", Value = JsonDocument.Parse("10")
        },
        new()
        {
            EntityType = ConfigType.Pagination, FieldName = "DefaultMaxPageSize", Value = JsonDocument.Parse("50")
        },
        new()
        {
            EntityType = ConfigType.Pagination, FieldName = "ConversationMaxPageSize", Value = JsonDocument.Parse("20")
        },
        new()
        {
            EntityType = ConfigType.Pagination, FieldName = "MessageMinPageSize", Value = JsonDocument.Parse("10")
        },
        new()
        {
            EntityType = ConfigType.Pagination, FieldName = "MessageMaxPageSize", Value = JsonDocument.Parse("100")
        },
        new()
        {
            EntityType = ConfigType.Cache, FieldName = "DefaultAbsoluteExpirationInMinutes",
            Value = JsonDocument.Parse("60")
        },
        new()
        {
            EntityType = ConfigType.Cache, FieldName = "DefaultSlidingExpirationInMinutes",
            Value = JsonDocument.Parse("30")
        },
        new()
        {
            EntityType = ConfigType.Package, FieldName = "MaximumPackageOfOneService", Value = JsonDocument.Parse("3")
        },
        new()
        {
            EntityType = ConfigType.Package, FieldName = "MaximumFeatureOfOnePackage", Value = JsonDocument.Parse("15")
        },
        new() { EntityType = ConfigType.Order, FieldName = "MaxSystemCancelPerYear", Value = JsonDocument.Parse("10") },
        new()
        {
            EntityType = ConfigType.Order, FieldName = "MaxSystemCancelBeforePenalty", Value = JsonDocument.Parse("3")
        },
        new()
        {
            EntityType = ConfigType.Order, FieldName = "PenaltyPercentageAfterCancel", Value = JsonDocument.Parse("20")
        },
        new() { EntityType = ConfigType.Order, FieldName = "OrderSuccessThreshold", Value = JsonDocument.Parse("3") },
        new()
        {
            EntityType = ConfigType.Reputation, FieldName = "MaxOrderPerMonthBasedOnReputation",
            Value = JsonDocument.Parse("6")
        },
        new()
        {
            EntityType = ConfigType.Reputation, FieldName = "MinReputationForVouchers", Value = JsonDocument.Parse("6")
        },
        new()
        {
            EntityType = ConfigType.Order, FieldName = "MaxCustomerOrdersPerMonth", Value = JsonDocument.Parse("3")
        },
        new()
        {
            EntityType = ConfigType.Reputation, FieldName = "ReputationIncreaseOnSuccess",
            Value = JsonDocument.Parse("2")
        },
        new()
        {
            EntityType = ConfigType.Reputation, FieldName = "MinReputationToAvoidBan", Value = JsonDocument.Parse("3")
        },
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
        ShipmentCode = "ORDCHD125FG8G2_Delivery",
        //CancleOrderReason = CancleOrderReason.ChangeShipmentAddress,
        DeliveryTime = 7,
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
        ShipmentCode = "ORDCHD125FG8G1_Delivery",
        //CancleOrderReason = CancleOrderReason.ChangeShipmentAddress,
        DeliveryTime = 7,
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

        foreach (var config in SystemConfigs)
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
            if (!context.Roles.Any(r => r.Id == role.Id && r.Name == role.Name))
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

        foreach (var accountRole in AccountRoles)
        {
            if (!context.AccountRoles.Any(c => c.Id == accountRole.Id))
            {
                accountRole.CreationDate = DateTime.UtcNow;
                context.AccountRoles.Add(accountRole);
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
            var existingFeature = context.Features.Local.FirstOrDefault(i => i.Id == feature.Id)
                                  ?? await context.Features.AsNoTracking().FirstOrDefaultAsync(i => i.Id == feature.Id);

            if (existingFeature == null)
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
