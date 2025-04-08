using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Repositories.Models.CategoryModels;
using Chillde.Repositories.Models.FAQModels;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.ItemModels;
using Chillde.Repositories.Models.MessageModels;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Repositories.Models.ShippingAddressModels;
using Chillde.Repositories.Models.SubCategoryModels;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.MessageModels;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ServiceAttachmentModels;
using Chillde.Services.Models.ServiceModels;
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
        CreateMap<AccountSignUpModel, Account>();
        CreateMap<AccountSignUpModel, Account>();
        CreateMap<Account, AccountModel>()
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src =>
                    src.AccountRoles.Select(accountRole => accountRole.Role.Name).Select(Enum.Parse<Role>)))
            .ForMember(dest => dest.RoleNames,
                opt => opt.MapFrom(src => src.AccountRoles.Select(accountRole => accountRole.Role.Name)))
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.Wallet.Balance))
            .ForMember(dest => dest.TotalReputations,
                opt => opt.MapFrom(src => src.AccountRoles.Select(accountRole => accountRole.TotalReputation)))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.AccountRoles.Select(accountRole => accountRole.Status)))
            .ForMember(dest => dest.StatusNames,
                opt => opt.MapFrom(src => src.AccountRoles.Select(accountRole => accountRole.Status.ToString())))
            .ForMember(dest => dest.OrderCount,
                opt => opt.MapFrom(src => src.Orders.Count)); // Đếm số lượng Orders

        // Message
        CreateMap<MessageAddModel, Message>();
        CreateMap<Message, MessageModel>();

        //Category
        CreateMap<CategoryModel,Category>().ReverseMap();
        CreateMap<CategoryAddModel,CategoryAddRangeModel>().ReverseMap();

        //FAQ
        CreateMap<FAQModel, FAQ>().ReverseMap();

        //Package
        CreateMap<PackageModel, Package>()
    .ForMember(dest => dest.ResponseTime, opt => opt.MapFrom(src => (float)src.ResponseTime.TotalMinutes))
    .ReverseMap()
    .ForMember(dest => dest.ResponseTime, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.ResponseTime)));
        CreateMap<PackageUpdateModel, Package>().ForMember(dest => dest.ResponseTime, opt => opt.MapFrom(src => (float)src.ResponseTime.TotalMinutes));
        CreateMap<PackageAddModel, Package>().ForMember(dest => dest.ResponseTime, opt => opt.MapFrom(src => (float)src.ResponseTime.TotalMinutes)).ReverseMap();

        //ShippingAddress
        CreateMap<ShippingAddress,ShippingAddressAddModel>().ReverseMap();
        CreateMap<ShippingAddress, ShippingAddressModel>().ReverseMap();
        CreateMap<ShippingAddressAddModel, ShippingAddressModel>().ReverseMap();
        CreateMap<ShippingAddressUpdateModel, ShippingAddress>().ReverseMap();

        //PackageFeature
        CreateMap<PackageFeatureUpdateModel, PackageFeature>().ReverseMap();
        CreateMap<PackageFeatureAddModel, PackageFeature>().ReverseMap();
        CreateMap<PackageFeatureModel, PackageFeature>().ReverseMap();

        //Feature
        CreateMap<FeatureModel, Feature>().ReverseMap();
        CreateMap<FeatureUpdateModel, Feature>().ReverseMap();

        //Service
        CreateMap<ServiceModel, Service>().ReverseMap();
        CreateMap<ServiceUpdateModel, Service>().ForMember(x => x.ServiceAttachments, otp => otp.Ignore()).ReverseMap();

        //Service
        CreateMap<ServiceAttachmentAddModel, ServiceAttachment>().ReverseMap();

        //Order
        CreateMap<OrderAddModel, Order>().ReverseMap();

        //Shipment
        CreateMap<ShipmentModel, Shipment>().ReverseMap();
    }
}