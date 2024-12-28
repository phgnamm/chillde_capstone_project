using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Repositories.Models.CategoryModels;
using Chillde.Repositories.Models.FAQModels;
using Chillde.Repositories.Models.ItemModels;
using Chillde.Repositories.Models.MessageModels;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Repositories.Models.SubCategoryModels;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.MessageModels;
using Chillde.Services.Models.ShippingAddressModels;
using Chillde.Services.Models.SubcategoryModels;
using Role = Chillde.Repositories.Enums.Role;

namespace Chillde.Services.Common;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // Account
        CreateMap<AccountSignUpModel, Account>();
        CreateMap<Account, AccountModel>()
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src =>
                    src.AccountRoles.Select(accountRole => accountRole.Role.Name).Select(Enum.Parse<Role>)))
            .ForMember(dest => dest.RoleNames,
                opt => opt.MapFrom(src => src.AccountRoles.Select(accountRole => accountRole.Role.Name)));
        CreateMap<Account, AccountLiteModel>();
        CreateMap<AccountUpdateModel, Account>();

        // Message
        CreateMap<MessageAddModel, Message>();
        CreateMap<Message, MessageModel>();

        //Category
        CreateMap<CategoryModel,Category>().ReverseMap();
        CreateMap<CategoryAddModel,CategoryAddRangeModel>().ReverseMap();

        //Subcaterory
        CreateMap<SubCategoryModel, SubCategory>().ReverseMap();
        CreateMap<SubCategoryAddModel, SubCategoryAddRangeModel>().ReverseMap();

        //Item
        CreateMap<ItemModel, Item>().ReverseMap();

        //FAQ
        CreateMap<FAQModel, FAQ>().ReverseMap();

        //Package
        CreateMap<PackageModel, Package>().ReverseMap();

        //ShippingAddress
        CreateMap<ShippingAddress,ShippingAddressAddModel>().ReverseMap();
    }
}