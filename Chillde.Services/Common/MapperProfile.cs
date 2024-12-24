using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.CategoryModels;
using Chillde.Repositories.Models.ItemModels;
using Chillde.Repositories.Models.MessageModels;
using Chillde.Repositories.Models.SubCategoryModels;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.MessageModels;
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
        CreateMap<CateroryModel,Category>().ReverseMap();

        //Subcaterory
        CreateMap<SubCategoryModel, SubCategory>().ReverseMap();

        //Item
        CreateMap<ItemModel, Item>().ReverseMap();
    }
}