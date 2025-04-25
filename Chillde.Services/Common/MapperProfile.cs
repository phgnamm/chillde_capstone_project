using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.AccountRoleModels;
using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Repositories.Models.CategoryModels;
using Chillde.Repositories.Models.DepositModels;
using Chillde.Repositories.Models.FAQModels;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.ItemModels;
using Chillde.Repositories.Models.MessageModels;
using Chillde.Repositories.Models.NotificationModels;
using Chillde.Repositories.Models.OfferModels;
using Chillde.Repositories.Models.OrderModels;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Repositories.Models.ReportAttachmentModels;
using Chillde.Repositories.Models.ReportModels;
using Chillde.Repositories.Models.ReputationLogModels;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Repositories.Models.ShippingAddressModels;
using Chillde.Repositories.Models.SubCategoryModels;
using Chillde.Repositories.Models.VoucherModels;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.MessageModels;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ReportModels;
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
            // .ForMember(dest => dest.Roles,
            //     opt => opt.MapFrom(src =>
            //         src.AccountRoles.Select(accountRole => accountRole.Role.Name).Select(Enum.Parse<Role>)))
            // .ForMember(dest => dest.RoleNames,
            //     opt => opt.MapFrom(src => src.AccountRoles.Select(accountRole => accountRole.Role.Name)))
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.Wallet.Balance))
            .ForMember(dest => dest.OrderCount,
                opt => opt.MapFrom(src => src.Orders.Count)) // Đếm số lượng Orders
            .ForMember(dest => dest.TotalAmountPaid,
                opt => opt.MapFrom(src =>
                     src.Wallet.Transactions
                    .Where(t => t.Type == TransactionType.TransferOut) // enum value == 3
                    .Sum(t => t.Amount ?? 0)))
            .ForMember(dest => dest.ServiceCount, opt => opt.MapFrom(src => src.Services.Count))
            .ForMember(dest => dest.AccountRoles, opt => opt.MapFrom(src => src.AccountRoles));
        CreateMap<Account, AccountLiteModel>();
        CreateMap<AccountUpdateModel, Account>();
        CreateMap<AccountBecomeASellerModel, Account>();

        // AccountRoles
        CreateMap<AccountRole, AccountRoleModel>()
            .ForMember(dest => dest.Role,
                opt => opt.MapFrom(src => Enum.Parse<Role>(src.Role.Name)))
            .ForMember(dest => dest.RoleName,
                opt => opt.MapFrom(src => src.Role.Name));

        // Message
        CreateMap<MessageAddModel, Message>();
        CreateMap<Message, MessageModel>();

        //Category
        CreateMap<CategoryModel, Category>().ReverseMap();
        CreateMap<CategoryAddModel, CategoryAddRangeModel>().ReverseMap();

        //FAQ
        CreateMap<FAQModel, FAQ>().ReverseMap();

        //Package
        CreateMap<PackageModel, Package>()
            .ForMember(dest => dest.ResponseTime, opt => opt.MapFrom(src => (float)src.ResponseTime.TotalMinutes))
            .ReverseMap()
            .ForMember(dest => dest.ResponseTime, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.ResponseTime)));
        CreateMap<PackageUpdateModel, Package>().ForMember(dest => dest.ResponseTime,
            opt => opt.MapFrom(src => (float)src.ResponseTime.TotalMinutes));
        CreateMap<PackageAddModel, Package>().ForMember(dest => dest.ResponseTime,
            opt => opt.MapFrom(src => (float)src.ResponseTime.TotalMinutes)).ReverseMap();
        CreateMap<PackageAddWithIdModel, Package>().ForMember(dest => dest.ResponseTime,
            opt => opt.MapFrom(src => (float)src.ResponseTime.TotalMinutes)).ReverseMap();

        //ShippingAddress
        CreateMap<ShippingAddress, ShippingAddressAddModel>().ReverseMap();
        CreateMap<ShippingAddress, ShippingAddressModel>().ReverseMap();
        CreateMap<ShippingAddressAddModel, ShippingAddressModel>().ReverseMap();
        CreateMap<ShippingAddressUpdateModel, ShippingAddress>().ReverseMap();

        //PackageFeature
        CreateMap<PackageFeatureUpdateModel, PackageFeature>().ReverseMap();
        CreateMap<PackageFeatureAddModel, PackageFeature>().ReverseMap();
        CreateMap<PackageFeatureAddModelForFeature, PackageFeature>().ReverseMap();
        CreateMap<PackageFeatureModel, PackageFeature>().ReverseMap();
        CreateMap<PackageFeatureAddModelForFeature, PackageFeatureAddModel>().ReverseMap();

        //Feature
        CreateMap<FeatureModel, Feature>().ReverseMap();
        CreateMap<FeatureAddModel, Feature>().ReverseMap();
        CreateMap<FeatureUpdateModel, Feature>().ReverseMap();
        CreateMap<FeatureUpdateModelForFeatureService, Feature>().ReverseMap();

        //Service
        CreateMap<ServiceModel, Service>().ReverseMap();
        CreateMap<ServiceUpdateModel, Service>().ForMember(x => x.ServiceAttachments, otp => otp.Ignore()).ReverseMap();

        //Service
        CreateMap<AttachmentAddModel, ServiceAttachment>().ReverseMap();

        //Order
        CreateMap<OrderAddModel, Order>().ReverseMap();
        CreateMap<OrderModel, Order>().ReverseMap();

        //Shipment
        CreateMap<ShipmentModel, Shipment>().ReverseMap();

        //Voucher
        CreateMap<VoucherModel, Voucher>().ReverseMap();
        CreateMap<Voucher, VoucherModel>().ReverseMap();

        //ReputationLog
        CreateMap<ReputationLogModel, ReputationLog>().ReverseMap();

        //Deposit
        CreateMap<DepositModel, Deposit>().ReverseMap();

        //Notification
        CreateMap<NotificationAddModel, Notification>().ReverseMap();
        CreateMap<Notification, NotificationModel>().ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.NotificationContent.Type)).ReverseMap();

        //Report
        CreateMap<ReportAddModel, Report>().ReverseMap();
        CreateMap<ReportModel, Report>().ReverseMap();
        CreateMap<ReportAttachmentModel, ReportAttachment>().ReverseMap();

        // Offer
        CreateMap<Offer, OfferModel>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
      .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
      .ForMember(dest => dest.MinWeight, opt => opt.MapFrom(src => src.MinWeight))
      .ForMember(dest => dest.MaxWeight, opt => opt.MapFrom(src => src.MaxWeight))
      .ForMember(dest => dest.OfferAttachments, opt => opt.MapFrom(src => src.OfferAttachments))
      .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.RequestId))
      .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId))
      .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
      .ForMember(dest => dest.ShippingAddress,
          opt => opt.MapFrom(src => src.CreatedBy.ShippingAddresses.FirstOrDefault()))
      .ForMember(dest => dest.Package, opt => opt.MapFrom(src => new PackageModel
      {
          Id = src.Package.Id,
          Name = src.Package.Name,
          Description = src.Package.Description,
          Price = src.Package.Price,
          DeliveryTime = src.Package.DeliveryTime,
          MaxQuantity = src.Package.MaxQuantity,
          SketchRevision = src.Package.SketchRevision,
          ResponseTime = TimeSpan.FromMinutes(src.Package.ResponseTime),
          Features = src.Package.PackageFeatures
              .Select(pf => pf.Feature)
              .Distinct()
              .Select(feature => new FeatureModel
              {
                  Id = feature.Id,
                  Name = feature.Name,
                  Question = feature.Question,
                  QuestionType = feature.QuestionType,
                  IsInformationRequired = feature.IsInformationRequired,
                  IsQuantity = feature.IsQuantity,
                  PackageFeatures = src.Package.PackageFeatures
                      .Where(pf => pf.FeatureId == feature.Id)
                      .Select(pf => new PackageFeature
                      {
                          Id = pf.Id,
                          Name = pf.Name,
                          AdditionalCost = pf.AdditionalCost,
                          AdditionalDay = pf.AdditionalDay,
                          IsExtra = pf.IsExtra,
                          IsChecked = pf.IsChecked,
                          MaxQuantity = pf.MaxQuantity,
                      }).ToList()
              }).ToList()
      }))
     .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));

    }
}