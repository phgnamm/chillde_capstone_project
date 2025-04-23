using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Repositories.Models.VnPayModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Chillde.Services.Common;
using Chillde.Repositories.Models.OrderModels;
using Chillde.Services.Helpers;
using Elasticsearch.Net;
using TransactionStatus = Chillde.Repositories.Enums.TransactionStatus;
using CloudinaryDotNet;
using StackExchange.Redis;
using Chillde.Repositories.Models.SystemConfigModel;
using System.Reflection.Metadata.Ecma335;
using CloudinaryDotNet.Core;
using Chillde.Repositories.Models.PackageModels;
using AutoMapper;
using Chillde.Repositories.Common;
using Chillde.Repositories.Models.OrderTrackingModels;
using Chillde.Services.Models.OrderTrackingModels;
using Chillde.Services.Models.CategoryModels;
using System.Linq.Expressions;
using Chillde.Repositories.Models.ServiceModels;
using CloudinaryDotNet.Actions;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.OfferModels;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Repositories.Models.FeatureModels;
using System.Net.WebSockets;
using static OpenAI.GPT3.ObjectModels.SharedModels.IOpenAiModels;
using System.Security.Principal;
using Nest;
using System;
using Chillde.Services.Models.ReportModels;
using Chillde.Repositories.Models.ReportModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Repositories.Models.ReportAttachmentModels;
using Chillde.Repositories.Models.NotificationModels;
using Chillde.Services.Models.ServiceAttachmentModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Chillde.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string? _shopId;
        private readonly string? _token;
        private readonly ISystemConfigService _systemConfigService;
        private readonly IEmailHelper _iIEmailHelper;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public OrderService(IEmailHelper iIEmailHelper, ISystemConfigService systemConfigService, IUnitOfWork unitOfWork, IClaimService claimService,
            ICloudinaryHelper cloudinaryHelper,
            IVnpay vnpay,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IMapper mapper,
            INotificationService notificationService
            )
        {
            _systemConfigService = systemConfigService;
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _vnpay = vnpay;
            _iIEmailHelper = iIEmailHelper;
            _httpClient = httpClientFactory.CreateClient("GhtkClient");
            _mapper = mapper;
            _notificationService = notificationService;
        }
        public async Task<ResponseModel> BalancePayment(OrderAddModel orderAddModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            var package = await _unitOfWork.PackageRepository.GetAsync(orderAddModel.PackageId,
                include: _ => _.Include(_ => _.PackageFeatures).ThenInclude(_ => _.Feature).Include(_ => _.Offer).ThenInclude(_ => _.Request));
            if (package == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Package not found"
                };
            var newOrder = await InitializeOrder(orderAddModel, package, currentUserId.Value);

            if (orderAddModel.OrderInformationAddModels != null && package.Offer == null)
                await ProcessExtraFeatures(orderAddModel, newOrder);
            if (orderAddModel.VoucherId != null && orderAddModel.VoucherId is List<Guid> voucherIds)
            {
                await ApplyVoucher(voucherIds, newOrder, PaymentType.Balance);
                foreach (var voucherUsageLog in newOrder.VoucherUsageLogs)
                {
                    voucherUsageLog.UsageStatus = UsageStatus.Used;
                }
            }
            var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId.Value, include: _ => _.Include(_ => _.Wallet));
            var wallet = account?.Wallet;
            if (wallet == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Wallet not found"
                };

            if (wallet.Balance < newOrder.TotalPrice)
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Your balance does not have enough money to complete this order"
                };

            wallet.Balance -= (decimal)newOrder.TotalPrice;

            newOrder.Transactions.Add(new Transaction
            {
                WalletId = wallet.Id,
                Amount = newOrder.TotalPrice,
                Type = TransactionType.TransferOut,
                Status = TransactionStatus.Completed,
                CreatedById = currentUserId.Value
            });
            newOrder.PaymentStatus = PaymentStatus.Success;
            _unitOfWork.WalletRepository.Update(wallet);
            await _unitOfWork.OrderRepository.AddAsync(newOrder);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result > 0)
            {
                var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Artisan_NewOrder).Result;
                if (notificationContent != null)
                {
                    var notificationAddModel = new NotificationAddModel
                    {
                        Content = notificationContent.Content.Replace("[#orderCode]", newOrder.Code),
                        AccountId = (Guid)(package.CreatedById!),
                        NotificationContentId = notificationContent.Id,
                        SourceId = newOrder.Id
                    };
                    await _notificationService.PushNotification(notificationAddModel);
                }
            }

            return result < 0 ?
                  new ResponseModel
                  {
                      Code = StatusCodes.Status400BadRequest,
                      Message = "Failed to process the payment"
                  }
                 :
                  new ResponseModel
                  {
                      Code = StatusCodes.Status200OK,
                      Message = "Payment successfully completed using balance",
                  };
        }
        public async Task<ResponseModel> CreatePaymentUrl(OrderAddModel orderAddModel, string ipAddress)
        {
            try
            {
                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized"
                    };
                }

                var package = await _unitOfWork.PackageRepository.GetAsync(orderAddModel.PackageId,
                    include: _ => _.Include(_ => _.PackageFeatures).ThenInclude(_ => _.Feature).Include(_ => _.Offer).ThenInclude(_ => _.Request));
                if (package == null)
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Package not found."
                    };


                var newOrder = await InitializeOrder(orderAddModel, package, currentUserId.Value);
                decimal remainingAmount = 0;

                if (orderAddModel.OrderInformationAddModels != null && package.Offer == null)
                    await ProcessExtraFeatures(orderAddModel, newOrder);
                if (orderAddModel.VoucherId != null)
                {
                    await ApplyVoucher((List<Guid>)orderAddModel.VoucherId, newOrder, PaymentType.VnPay);
                }
                var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId.Value, include: _ => _.Include(_ => _.Wallet));
                var wallet = account?.Wallet;
                if ((bool)orderAddModel.WithBalance)
                {
                    var response = await ProcessWalletPayment(wallet, newOrder, currentUserId.Value, wallet.Id);
                    if (response.Data != null)
                    {
                        remainingAmount = (decimal)response.Data;
                        newOrder.WithBalance = true;
                    }
                    else return response;
                }
                if (!(bool)orderAddModel.WithBalance)
                {
                    decimal depositAmount = (decimal)newOrder.TotalPrice;
                    wallet.Balance += depositAmount;
                    var deposit = new Deposit
                    {
                        Amount = depositAmount,
                        Type = DepositType.Deposit,
                        CreatedById = account.Id,
                        Status = DepositStatus.Pending,
                        WalletId = wallet.Id,
                        OrderId = newOrder.Id

                    };
                    wallet.Deposits.Add(deposit);
                    var transaction = new Transaction
                    {
                        Amount = depositAmount,
                        Type = TransactionType.TransferOut,
                        CreatedById = account.Id,
                        Status = TransactionStatus.Pending,
                        WalletId = wallet.Id
                    };
                    newOrder.Transactions.Add(transaction);
                    wallet.Balance -= (decimal)newOrder.TotalPrice;
                    _unitOfWork.WalletRepository.Update(wallet);
                }
                await _unitOfWork.OrderRepository.AddAsync(newOrder);

                if (await _unitOfWork.SaveChangeAsync() < 0)
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Fail to save order"
                    };

                var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Artisan_NewOrder).Result;
                if (notificationContent != null)
                {
                    var notificationAddModel = new NotificationAddModel
                    {
                        Content = notificationContent.Content.Replace("[#orderCode]", newOrder.Code),
                        AccountId = (Guid)(package.CreatedById!),
                        NotificationContentId = notificationContent.Id,
                        SourceId = newOrder.Id
                    };
                    await _notificationService.PushNotification(notificationAddModel);
                }

                var paymentUrl = await GenerateVnPayUrl(newOrder, ipAddress, (decimal)remainingAmount);

                return new ResponseModel { Data = paymentUrl, Message = "Created paymentUrl successfully" };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<Repositories.Entities.Order> InitializeOrder(OrderAddModel orderAddModel, Package package, Guid userId)
        {
            decimal totalOrder;
            decimal adminCommission;

            if (package.Offer?.Status == OfferStatus.Approved)
            {
                totalOrder = (decimal)(package.Price * package.Offer.Request.Quantity);
                adminCommission = await AdminCommission(totalOrder, 0);

                return await CreateOrderAsync(orderAddModel, package, userId, totalOrder, adminCommission, (int)(package.Offer?.Request?.Quantity ?? 1), null, null);

            }

            var requiredFeatures = package.PackageFeatures
                 .Where(_ => _.Feature.IsInformationRequired && (!_.IsExtra ?? true))
                 .ToList();
            var requiredFeatureIds = requiredFeatures.Select(_ => _.FeatureId).Distinct().ToList();
            var providedFeatureIds = orderAddModel.OrderInformationAddModels
                         .Join(package.PackageFeatures,
                            orderInfo => orderInfo.PackageFeatureId,
                            packageFeature => packageFeature.Id,
                            (orderInfo, packageFeature) => new { orderInfo, packageFeature })
                        .Select(_ => _.packageFeature.FeatureId)
                        .ToList();
            ///////
            var missingFeatureIds = requiredFeatureIds
                .Where(id => !providedFeatureIds.Contains(id))
                .ToList();
            //extraFeatureCost = extraFeatureIds.Data.Sum(pf =>
            //      orderAddModel.OrderInformationAddModels!
            //     .Where(_ => _.PackageFeatureId == pf.Id)
            //     .Sum(_ => (_.Quantity ?? 1) * (pf.AdditionalCost ?? 0))
            //       );
            //extraFeatureDeliveryTime = takeExtraFeature.Data.Sum(pf =>
            //   orderAddModel.OrderInformationAddModels!
            //       .Where(_ => _.PackageFeatureId == pf.Id)
            //       .Sum(_ => (pf.AdditionalDay ?? 0))
            // );
            //if (extraFeatureCost > 0)
            //{
            //    newOrder.DeliveryTime += extraFeatureDeliveryTime;
            //    newOrder.TotalPrice += (decimal)(extraFeatureCost * newOrder.Quantity);
            //    newOrder.OriginPrice += (decimal)(extraFeatureCost * newOrder.Quantity);
            //    var newCommission = await AdminCommission((decimal)((decimal)newOrder.TotalPrice - newOrder.ShippingPrice), 0);
            //    newOrder.AdminCommDefault = newCommission;
            //    newOrder.ArtistRevenue = (newOrder.TotalPrice - newOrder.ShippingPrice - newCommission);
            //}
            ///////
            if (missingFeatureIds.Any())
            {
                throw new InvalidOperationException($"Missing required features: {string.Join(", ", missingFeatureIds)}");

            }
            foreach (var requiredFeature in requiredFeatures)
            {
                var orderInfo = orderAddModel.OrderInformationAddModels
                    ?.FirstOrDefault(_ => _.PackageFeatureId == requiredFeature.Id);

                if (requiredFeature.Feature.QuestionType == MediaType.Text)
                {
                    if (orderInfo == null || string.IsNullOrWhiteSpace(orderInfo.Description))
                    {
                        throw new InvalidOperationException($"Required description for feature '{requiredFeature.Feature.Name}' is missing.");
                    }
                }
                else
                {
                    if (orderInfo != null && string.IsNullOrWhiteSpace(orderInfo.Description))
                    {
                        orderInfo.Description = requiredFeature.Name;
                    }
                }
            }

            totalOrder = (decimal)(package.Price * orderAddModel.Quantity);
            adminCommission = await AdminCommission(totalOrder, 0);

            return await CreateOrderAsync(
                         orderAddModel,
                         package,
                         userId,
                         totalOrder,
                         adminCommission,
                         (int)(orderAddModel.Quantity ?? 1),
                         orderAddModel.OrderInformationAddModels,
                         orderAddModel.OrderInformationAddModels?
                        .SelectMany(_ => _.OrderInformationAttachmentAddModels ?? new List<OrderInformationAttachmentAddModel>())
             );

        }

        private async Task<Repositories.Entities.Order> CreateOrderAsync(
               OrderAddModel orderAddModel,
               Package package,
               Guid userId,
               decimal totalOrder,
               decimal adminCommission,
               int quantity,
               IEnumerable<OrderInformationAddModel>? orderInformationAddModels,
               IEnumerable<OrderInformationAttachmentAddModel>? orderInformationAttachmentAddModels)
        {
            var order = new Repositories.Entities.Order
            {
                Id = Guid.NewGuid(),
                CreatedById = userId,
                Code = GenerateCodeHelper.GenerateOrderCode(),
                Phone = orderAddModel.Phone,
                Address = orderAddModel.Address,
                ToWard = orderAddModel.ToWard,
                ToDistrict = orderAddModel.ToDistrict,
                ToProvince = orderAddModel.ToProvince,
                TotalPrice = totalOrder + (orderAddModel.ShippingPrice ?? 0),
                DeliveryTime = package.DeliveryTime,
                ShippingPrice = orderAddModel.ShippingPrice ?? 0,
                OriginPrice = totalOrder,
                CurrentSketchRevision = package.SketchRevision,
                AdminCommDefault = adminCommission,
                AdminCommUsedVch = null,
                ArtistRevenue = totalOrder - adminCommission,
                AfterApplyVoucherPrice = null,
                VoucherCost = null,
                Quantity = quantity,
                PackageId = package.Id,
                WithBalance = false,
                OrderInformations = new List<OrderInformation>()
            };

            if (orderInformationAddModels != null)
            {
                foreach (var info in orderInformationAddModels)
                {
                    var packageFeature = await _unitOfWork.PackageFeatureRepository.GetAsync(info.PackageFeatureId);
                    var orderInfo = new OrderInformation
                    {
                        Quantity = info.Quantity ?? 1,
                        Price = packageFeature.AdditionalCost ?? 0,
                        Description = info.Description ?? "",
                        PackageFeatureId = info.PackageFeatureId,
                        OrderInformationAttachments = new List<OrderInformationAttachment>()
                    };

                    if (info.OrderInformationAttachmentAddModels != null)
                    {
                        foreach (var attachment in info.OrderInformationAttachmentAddModels)
                        {
                            if (attachment.AttachmentUrl != null)
                            {
                                var attachmentPath = await _cloudinaryHelper.UploadImageAsync(
                                    attachment.AttachmentUrl,
                                    order.Code,
                                    folderName: FolderAttachment.ORDERATTACHMENT
                                );

                                orderInfo.OrderInformationAttachments.Add(new OrderInformationAttachment
                                {
                                    AttachmentUrl = attachmentPath ?? "Unknown",
                                    AttachmentAlt = attachment.AttachmentAlt ?? "Unknown"
                                });
                            }
                        }
                    }

                    order.OrderInformations.Add(orderInfo);
                }
            }

            return order;
        }


        private async Task<decimal> AdminCommission(decimal totalOrder, decimal? commsionVoucherValue)
        {
            var commissionResponse = await _systemConfigService.Get(SystemConfigKey.Commission);
            if (commissionResponse.Data is SystemConfigModel config && decimal.TryParse((string?)config.Value, out decimal commissionValue))
            {
                if (commissionValue > 0)
                {
                    var adjustedCommission = (commissionValue - (commsionVoucherValue ?? 0)) / 100;
                    return totalOrder * adjustedCommission;
                }

                return totalOrder * (commissionValue / 100);
            }
            return 0;
        }
        private async Task ProcessExtraFeatures(OrderAddModel orderAddModel, Repositories.Entities.Order newOrder)
        {
            var extraFeatureIds = orderAddModel.OrderInformationAddModels?
                                    .Select(_ => _.PackageFeatureId)
                                    .ToList() ?? new List<Guid>();

            var takeExtraFeature = await _unitOfWork.PackageFeatureRepository.GetAllAsync(
                filter: _ => extraFeatureIds.Contains(_.Id) && _.IsExtra == true
            );
            if (takeExtraFeature?.Data == null || !takeExtraFeature.Data.Any())
            {
                return;
            }
            //var checkMaxQuantity = takeExtraFeature.Data.Where(pf =>
            //    orderAddModel.OrderInformationAddModels!
            //        .Any(_ => _.PackageFeatureId == pf.Id && _.Quantity > pf.MaxQuantity));
            //if (checkMaxQuantity != null)
            //{
            //    throw new Exception("Quantity in order information cannot greater than max quantity in feature package");
            //}
            var extraFeatureCost = takeExtraFeature.Data.Sum(pf =>
                orderAddModel.OrderInformationAddModels!
                    .Where(_ => _.PackageFeatureId == pf.Id)
                    .Sum(_ => (_.Quantity ?? 1) * (pf.AdditionalCost ?? 0))
            );
            var extraFeatureDeliveryTime = takeExtraFeature.Data.Sum(pf =>
                orderAddModel.OrderInformationAddModels!
                    .Where(_ => _.PackageFeatureId == pf.Id)
                    .Sum(_ => (pf.AdditionalDay ?? 0))
            );
            if (extraFeatureCost > 0)
            {
                newOrder.DeliveryTime += extraFeatureDeliveryTime;
                newOrder.TotalPrice += (decimal)(extraFeatureCost * newOrder.Quantity);
                newOrder.OriginPrice += (decimal)(extraFeatureCost * newOrder.Quantity);
                var newCommission = await AdminCommission((decimal)((decimal)newOrder.TotalPrice - newOrder.ShippingPrice), 0);
                newOrder.AdminCommDefault = newCommission;
                newOrder.ArtistRevenue = (newOrder.TotalPrice - newOrder.ShippingPrice - newCommission);
            }
        }
        private async Task ApplyVoucher(List<Guid> voucherIds, Repositories.Entities.Order order, PaymentType? paymentType)
        {
            if (voucherIds.Distinct().Count() != voucherIds.Count)
            {
                throw new Exception("Duplicate vouchers are not allowed.");
            }
            var vouchers = await _unitOfWork.VoucherRepository.GetAllAsync(
                filter: _ => voucherIds.Contains(_.Id)
            );

            if (vouchers == null || !vouchers.Data.Any())
            {
                throw new Exception("No valid vouchers found.");
            }

            decimal remainingOrderPrice = (decimal)order.OriginPrice;
            decimal totalVoucherCost = 0;

            foreach (var voucherId in voucherIds)
            {
                var voucher = vouchers.Data.FirstOrDefault(_ => _.Id == voucherId);
                if (voucher == null) throw new Exception("No valid vouchers found.");


                if (voucher.MinOrderValue.HasValue && remainingOrderPrice < voucher.MinOrderValue.Value)
                {
                    throw new Exception($"The order has at least {voucher.MinOrderValue} to apply this voucher.");
                }

                if (voucher.RemainingQuantity.HasValue && voucher.RemainingQuantity.Value < 1)
                {
                    throw new Exception("This voucher is out of stock to use.");

                }
                if (voucher.ExpiredTime < DateTime.Now)
                {
                    throw new Exception("This voucher has expired.");
                }
                decimal discount = remainingOrderPrice * (voucher.DiscountValue / 100);
                if (voucher.MaxDiscountValue.HasValue && discount > voucher.MaxDiscountValue.Value)
                {
                    discount = voucher.MaxDiscountValue.Value;
                }

                remainingOrderPrice -= discount;
                totalVoucherCost += discount;

                order.VoucherUsageLogs.Add(new VoucherUsageLog
                {
                    VoucherId = voucher.Id,
                    CreatedById = (Guid)order.CreatedById,
                    DiscountValue = discount,
                    DiscountValueOrigin = voucher.DiscountValue,
                });
                if (paymentType == PaymentType.Balance)
                {
                    if (voucher.TotalQuantity.HasValue && voucher.RemainingQuantity >= 1)
                    {
                        voucher.RemainingQuantity -= 1;
                        if (voucher.RemainingQuantity == 0)
                        {
                            voucher.VoucherStatus = VoucherStatus.OutOfStock;
                        }
                    }
                }
                order.AfterApplyVoucherPrice = remainingOrderPrice;
                order.VoucherCost = totalVoucherCost;
                var adminCommAfterUsedVch = await AdminCommission((decimal)remainingOrderPrice, 0);
                order.AdminCommDefault = adminCommAfterUsedVch;
                order.ArtistRevenue = remainingOrderPrice - adminCommAfterUsedVch;
                order.TotalPrice = remainingOrderPrice + order.ShippingPrice;
                _unitOfWork.VoucherRepository.Update(voucher);
            }


        }
        private async Task<ResponseModel> ProcessWalletPayment(Wallet wallet, Repositories.Entities.Order order, Guid accountId, Guid walletId)
        {
            var balance = wallet.Balance;

            if (balance <= 0)
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Your balance does not have enough money to order"
                };

            if (balance > order.TotalPrice)
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Your balance is greater than the total price in the order. Do you want to checkout with balance payment!"
                };

            var remainingAmount = order.TotalPrice - balance;
            if (remainingAmount < 5000)
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = $"Cannot checkout VNPay with '{remainingAmount} VND'. Please checkout with just vnPay payment!"
                };
            decimal depositAmount = (decimal)order.TotalPrice - wallet.Balance;
            var deposit = new Deposit
            {
                Amount = depositAmount,
                Type = DepositType.Deposit,
                CreatedById = accountId,
                Status = DepositStatus.Pending,
                WalletId = wallet.Id,
                OrderId = order.Id
            };

            var transaction = new Transaction
            {
                Amount = wallet.Balance + depositAmount,
                Type = TransactionType.TransferOut,
                CreatedById = accountId,
                Status = TransactionStatus.Pending,
                WalletId = wallet.Id
            };
            order.Transactions.Add(transaction);
            wallet.Deposits.Add(deposit);
            _unitOfWork.WalletRepository.Update(wallet);

            return new ResponseModel { Data = remainingAmount };
        }
        private async Task<string> GenerateVnPayUrl(Repositories.Entities.Order order, string ipAddress, decimal remainingAmount)
        {
            var paymentRequest = new PaymentRequest
            {
                OrderId = order.Id,
                Money = (double)(remainingAmount > 0 ? remainingAmount : order.TotalPrice),
                Description = $"Payment for order {order.Id}",
                IpAddress = ipAddress,
                BankCode = BankCode.ANY,
                CreatedDate = DateTime.Now,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese
            };

            return await _vnpay.GetPaymentUrl(paymentRequest);
        }
        public async Task<ResponseModel> UpdateOrderStatusToCompleted(Guid orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetAsync(
                orderId,
                _ => _.Include(_ => _.CreatedBy)
                      .ThenInclude(_ => _.Wallet).ThenInclude(_ => _.Deposits)
                      .Include(_ => _.Transactions)
                      .Include(_ => _.VoucherUsageLogs).ThenInclude(_ => _.Voucher)
            );

            if (order == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order not found."
                };

            if (order.Status == OrderStatus.Accepted)
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Order is already completed."
                };

            foreach (var voucherUsageLog in order.VoucherUsageLogs)
            {
                if (voucherUsageLog.Voucher.TotalQuantity.HasValue && voucherUsageLog.Voucher.RemainingQuantity >= 1)
                {
                    voucherUsageLog.Voucher.RemainingQuantity -= 1;
                    voucherUsageLog.UsageStatus = UsageStatus.Used;
                }
            }

            order.PaymentStatus = PaymentStatus.Success;
            var wallet = order.CreatedBy.Wallet;

            if (wallet == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Wallet not found for the user."
                };
            var transferOut = order.Transactions.FirstOrDefault(_ => _.Type == TransactionType.TransferOut);
            var account = order.CreatedBy;
            foreach (var transaction in order.Transactions)
            {
                transaction.Status = TransactionStatus.Completed;
            }
            var deposit = wallet.Deposits.Where(_ => _.OrderId == order.Id).FirstOrDefault();
            deposit.Status = DepositStatus.Success;

            if ((bool)order.WithBalance)
            {
                wallet.Balance = (decimal)(transferOut.Amount - order.TotalPrice);
                _unitOfWork.WalletRepository.Update(wallet);
            }

            _unitOfWork.WalletRepository.Update(wallet);

            _unitOfWork.OrderRepository.Update(order);
            var result = await _unitOfWork.SaveChangeAsync();

            return result > 0 ? new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Order status updated to completed successfully."
            } : new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = "Failed to update order status and wallet."
            };
        }
        public async Task<ResponseModel> CreateShipmentAsync(ShipmentCreateModel shipmentCreateModel, Guid orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetAsync(orderId, include: 
                order => order.Include(_ => _.Package).ThenInclude(_ => _.Service)
                              .Include(_ => _.Package).ThenInclude(_ => _.Offer));
            if (order == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order not found."
                };
            }


            if (order.Stage != OrderStage.Shipping && order.Stage != OrderStage.Cancelled)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status422UnprocessableEntity,
                    Message = "Shipment can only be initiated at the delivery and return stage."
                };
            }

            string partnerId = $"{order.Code}_{order.Stage.GetStringValue()}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            string partnerIdWithOutTime = partnerId.Substring(0, partnerId.IndexOf('_', partnerId.IndexOf('_') + 1));

            var availableShipment = _unitOfWork.ShipmentRepository.HasAvalaibleShipment(orderId, partnerIdWithOutTime);

            if (availableShipment)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status422UnprocessableEntity,
                    Message = $"{order.Stage.GetStringValue()} stage already had shipment."
                };
            }

            if (shipmentCreateModel == null || !shipmentCreateModel.Products.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid shipment data",
                    Data = null
                };
            }

            var url = "https://services-staging.ghtklab.com/services/shipment/order";
            var jsonBody = JsonConvert.SerializeObject(new
            {
                products = shipmentCreateModel.Products,
                order = new
                {
                    id = partnerId,
                    pick_name = shipmentCreateModel.PickName,
                    pick_address = shipmentCreateModel.PickAddress,
                    pick_province = shipmentCreateModel.PickProvince,
                    pick_district = shipmentCreateModel.PickDistrict,
                    pick_ward = shipmentCreateModel.PickWard,
                    pick_tel = shipmentCreateModel.PickTel,
                    name = shipmentCreateModel.Name,
                    address = shipmentCreateModel.Address,
                    province = shipmentCreateModel.Province,
                    district = shipmentCreateModel.District,
                    ward = shipmentCreateModel.Ward,
                    tel = shipmentCreateModel.Tel,
                    hamlet = shipmentCreateModel.Hamlet,
                    email = shipmentCreateModel.Email,
                    //return_name = shipmentCreateModel.ReturnName,
                    //return_address = shipmentCreateModel.ReturnAddress,
                    //return_province = shipmentCreateModel.ReturnProvince,
                    //return_district = shipmentCreateModel.ReturnDistrict,
                    //return_tel = shipmentCreateModel.ReturnTel,
                    //return_email = shipmentCreateModel.ReturnEmail,
                    is_freeship = /*shipmentCreateModel.IsFreeShip*/1,
                    /* pick_date = shipmentCreateModel.PickDate,
                     deliver_date = shipmentCreateModel.DeliverDate,*/
                    pick_money = /*shipmentCreateModel.PickMoney*/0,
                    note = shipmentCreateModel.Note,
                    value = shipmentCreateModel.Value,
                    transport = /*shipmentCreateModel.Transport*/"road",
                    pick_option = /*shipmentCreateModel.PickOption*/"cod",
                    deliver_option = /*shipmentCreateModel.DeliverOption*/"none",
                    tags = shipmentCreateModel.Tags
                }
            }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
            {
                Content = content
            };
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                var parsedJson = JsonConvert.DeserializeObject<ShipmentAddResponseModel>(responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel
                    {
                        Code = (int)response.StatusCode,
                        Message = parsedJson.Success,
                        Data = parsedJson
                    };
                }

                Shipment shipment = new()
                {
                    OrderId = order.Id,
                    TrackingId = parsedJson!.Order!.TrackingId.ToString(),
                    CurrentStatusId = (ShipmentStatus)(parsedJson.Order?.StatusId ?? 0),
                    PartnerId = parsedJson!.Order!.PartnerId,
                    Label = parsedJson.Order.Label,
                    Area = parsedJson.Order.Area,
                    Fee = parsedJson.Order.Fee != null ? decimal.Parse(parsedJson.Order.Fee) : 0,
                    InsuranceFee = parsedJson.Order.InsuranceFee != null ? decimal.Parse(parsedJson.Order.InsuranceFee) : 0,
                    EstimatedPickTime = parsedJson.Order.EstimatedPickTime,
                    EstimatedDeliverTime = parsedJson.Order.EstimatedDeliverTime,
                };
                if (shipmentCreateModel.Products != null && shipmentCreateModel.Products.Any())
                {
                    foreach (var product in shipmentCreateModel.Products)
                    {
                        shipment.ProductShipments.Add(new ProductShipment
                        {
                            ShipmentId = shipment.Id,
                            Name = product.Name ?? string.Empty,
                            Weight = product.Weight,
                            Quantity = product.Quantity ?? 0,
                            ProductCode = string.Empty 
                        });
                    }
                }
                shipment.ShipmentStatusHistorys.Add(new ShipmentStatusHistory
                {
                    ShipmentId = shipment.Id,
                    StatusId = shipment.CurrentStatusId,
                });


                await _unitOfWork.ShipmentRepository.AddAsync(shipment);
                int result = await _unitOfWork.SaveChangeAsync();
                if(result > 0)
                {
                    var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Customer_InDelivery).Result;
                    if (notificationContent != null)
                    {
                        var notificationAddModel = new NotificationAddModel
                        {
                            Content = notificationContent.Content.Replace("[#orderCode]", order.Code),
                            AccountId = (Guid)(order.Package.Service != null ? order.Package.Service.CreatedById : order.Package.Offer?.CreatedById)!,
                            NotificationContentId = notificationContent.Id,
                            SourceId = order.Id
                        };
                        await _notificationService.PushNotification(notificationAddModel);
                    }
                }
                var shipmentModel = _mapper.Map<ShipmentModel>(shipment);

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Success",
                    Data = shipmentModel
                };

                //var jsonObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                //if (jsonObject?["success"]?.Value<bool>() != true)
                //{
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status400BadRequest,
                //        Message = jsonObject?["message"]?.ToString() ?? "Unknown error",
                //        Data = null
                //    };
                //}

                //var orderStatusResponse = jsonObject["order"]?.ToObject<ShipmentAddResponseModel>();
                //return new ResponseModel
                //{
                //    Code = StatusCodes.Status200OK,
                //    Message = "Success",
                //    Data = orderStatusResponse
                //};
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
        public async Task<ResponseModel> CancelShipmentAsync(Guid orderId, string shipmentCode)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetAsync(orderId);
                if (order == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found."
                    };
                }

                if (order.ShipmentCode == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status406NotAcceptable,
                        Message = "Đơn hàng chưa có đơn vận chuyển"
                    };
                }

                var customer = await _unitOfWork.AccountRepository.GetAsync((Guid)order.CreatedById);
                if (customer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found."
                    };
                }

                _httpClient.DefaultRequestHeaders.Clear();

                var payload = new
                {
                    order_codes = new[] { order.ShipmentCode }
                };

                //var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                //var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                //var response = await _httpClient.PostAsync(_url, content);

                //var responseContent = await response.Content.ReadAsStringAsync();
                //var result = JsonConvert.DeserializeObject<ShipmentResponseModel>(responseContent);

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);
                _httpClient.DefaultRequestHeaders.Add("Token", _token);
                var response = await _httpClient.PostAsync("v2/switch-status/cancel", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ShipmentCancelResponseModel>(responseContent);

                if (result.Code == StatusCodes.Status200OK.ToString())
                {
                    order.ShipmentCode = null;
                }

                _unitOfWork.OrderRepository.Update(order);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseModel> GetAll(OrderFilterModel orderFilterModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            if (!Enum.IsDefined(typeof(Repositories.Enums.Role), orderFilterModel.Role))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid role"
                };
            }

            if (orderFilterModel.PageIndex < 0 || orderFilterModel.PageSize <= 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid pagination parameters"
                };
            }

            Func<IQueryable<Repositories.Entities.Order>, IOrderedQueryable<Repositories.Entities.Order>> orderBy = query =>
            {
                switch (orderFilterModel.Order?.ToLower())
                {
                    case "recentdays":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderByDescending(o => o.CreationDate)
                            : query.OrderBy(o => o.CreationDate);
                    case "olddays":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.CreationDate)
                            : query.OrderByDescending(o => o.CreationDate);
                    case "code":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.Code)
                            : query.OrderByDescending(o => o.Code);
                    case "servicename":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.Package.Service.Name)
                            : query.OrderByDescending(o => o.Code);
                    case "customername":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.CreatedBy.Username)
                            : query.OrderByDescending(o => o.CreatedBy.Username);
                    case "packagename":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.Package.Name)
                            : query.OrderByDescending(o => o.Package.Name);
                    case "artistname":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.Package.Service != null ? o.Package.Service.CreatedBy.Username : o.Package.Offer.CreatedBy.Username)
                            : query.OrderByDescending(o => o.Package.Service != null ? o.Package.Service.CreatedBy.Username : o.Package.Offer.CreatedBy.Username);
                    case "status":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.Status)
                            : query.OrderByDescending(o => o.Status);
                    case "totalprice":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.TotalPrice)
                            : query.OrderByDescending(o => o.TotalPrice);
                    case "artistrevenue":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.ArtistRevenue)
                            : query.OrderByDescending(o => o.ArtistRevenue);
                    case "admincommission":
                        return orderFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.AdminCommDefault ?? o.AdminCommUsedVch)
                            : query.OrderByDescending(o => o.AdminCommDefault ?? o.AdminCommUsedVch);
                    default:
                        return orderFilterModel.OrderByDescending
                            ? query.OrderByDescending(o => o.CreationDate)
                            : query.OrderBy(o => o.CreationDate);
                }
            };

            Expression<Func<Chillde.Repositories.Entities.Order, bool>> filter = orderFilterModel.Role switch
            {
               Chillde.Repositories.Enums.Role.Customer => o =>
                    o.CreatedById == orderFilterModel.AccountId &&
                    (!orderFilterModel.Status.HasValue || o.Status == orderFilterModel.Status) &&
                    (!orderFilterModel.MinPrice.HasValue || o.TotalPrice >= orderFilterModel.MinPrice) &&
                    (!orderFilterModel.MaxPrice.HasValue || o.TotalPrice <= orderFilterModel.MaxPrice) &&
                    (string.IsNullOrEmpty(orderFilterModel.Search) || (
                        o.Code.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.Username.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.PhoneNumber.Contains(orderFilterModel.Search) ||
                        o.Address.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.FirstName.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.LastName.Contains(orderFilterModel.Search)
                    )),

                Chillde.Repositories.Enums.Role.Artisan => o =>
                    (
                        (o.Package.Service != null && o.Package.Service.CreatedById == orderFilterModel.AccountId) ||
                        (o.Package.Offer != null && o.Package.Offer.CreatedById == orderFilterModel.AccountId)
                    ) &&
                    (!orderFilterModel.Status.HasValue || o.Status == orderFilterModel.Status) &&
                    (!orderFilterModel.MinPrice.HasValue || o.TotalPrice >= orderFilterModel.MinPrice) &&
                    (!orderFilterModel.MaxPrice.HasValue || o.TotalPrice <= orderFilterModel.MaxPrice) &&
                    (string.IsNullOrEmpty(orderFilterModel.Search) || (
                        o.Code.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.Username.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.PhoneNumber.Contains(orderFilterModel.Search) ||
                        o.Address.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.FirstName.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.LastName.Contains(orderFilterModel.Search)
                    )),

                Chillde.Repositories.Enums.Role.Admin => o =>
                    (!orderFilterModel.Status.HasValue || o.Status == orderFilterModel.Status) &&
                    (!orderFilterModel.AccountId.HasValue || o.CreatedById == orderFilterModel.AccountId) &&
                    (!orderFilterModel.MinPrice.HasValue || o.TotalPrice >= orderFilterModel.MinPrice) &&
                    (!orderFilterModel.MaxPrice.HasValue || o.TotalPrice <= orderFilterModel.MaxPrice) &&
                    (string.IsNullOrEmpty(orderFilterModel.Search) || (
                        o.Code.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.Username.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.PhoneNumber.Contains(orderFilterModel.Search) ||
                        o.Address.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.FirstName.Contains(orderFilterModel.Search) ||
                        o.CreatedBy.LastName.Contains(orderFilterModel.Search)
                    )),

                _ => o => false
            };


            try
            {
                var orders = await _unitOfWork.OrderRepository.GetAllAsync(
                    filter: filter,
                    include: q => q.Include(o => o.CreatedBy)
                                   .Include(o => o.Package)
                                   .ThenInclude(p => p.Service).ThenInclude(s => s.CreatedBy)
                                   .Include(o => o.Package)
                                   .ThenInclude(p => p.Service).ThenInclude(s => s.ServiceAttachments)
                                   .Include(o => o.Package)
                                   .ThenInclude(p => p.Offer).ThenInclude(o => o.OfferAttachments)
                                   .Include(o => o.Package)
                                   .ThenInclude(p => p.Offer).ThenInclude(o => o.CreatedBy)
                                   .Include(o => o.Shipments),
                    order: orderBy,
                    pageIndex: orderFilterModel.PageIndex,
                    pageSize: orderFilterModel.PageSize
                );

                var orderModels = orders.Data.Select(order => new OrderModel
                {
                    Id = order.Id,
                    Name = order.Package.ServiceId.HasValue && order.Package.Service != null
                        ? order.Package.Service.Name ?? "Unknown"
                        : order.Package.OfferId.HasValue && order.Package.Offer != null
                            ? order.Package.Offer.Message ?? "Unknown"
                            : "Unknown",
                    ShipmentId = order.Shipments?.FirstOrDefault()?.Id.ToString() ?? string.Empty,
                    Attachments = order.Package.ServiceId.HasValue && order.Package.Service != null
                        ? order.Package.Service.ServiceAttachments?.Select(att => new ServiceAttachment
                        {
                            Id = att.Id,
                            AttachmentUrl = att.AttachmentUrl ?? string.Empty,
                            AttachmentAlt = att.AttachmentAlt ?? string.Empty,
                            ServiceId = att.ServiceId
                        }).ToList() ?? new List<ServiceAttachment>()
                        : order.Package.OfferId.HasValue && order.Package.Offer != null
                            ? order.Package.Offer.OfferAttachments?.Select(att => new ServiceAttachment
                            {
                                Id = att.Id,
                                AttachmentUrl = att.AttachmentUrl ?? string.Empty,
                                AttachmentAlt = att.AttachmentAlt ?? string.Empty,
                                ServiceId = Guid.Empty
                            }).ToList() ?? new List<ServiceAttachment>()
                            : new List<ServiceAttachment>(),
                    Code = order.Code,
                    CustomerName = order.CreatedBy.FirstName + " " + order.CreatedBy.LastName,
                    Phone = order.Phone ?? string.Empty,
                    Address = order.Address ?? string.Empty,
                    ToDistrict = order.ToDistrict,
                    ToProvince = order.ToProvince ?? string.Empty,
                    ToWard = order.ToWard ?? string.Empty,
                    TotalPrice = order.TotalPrice ?? 0,
                    PackagePrice = order.Package.Price ?? 0,
                    AdminCommDefault = order.AdminCommDefault,
                    AdminCommUsedVch = order.AdminCommUsedVch,
                    AdminComm = order.AdminCommDefault ?? order.AdminCommUsedVch,
                    ArtistRevenue = order.ArtistRevenue,
                    PackageName = order.Package.Name,
                    Quantity = order.Quantity ?? 1,
                    ShipmentCode = order.ShipmentCode ?? string.Empty,
                    OrderStage = order.Stage,
                    Status = order.Status,
                    CreatedById = order.CreatedById,
                    Artisan = order.Package.ServiceId.HasValue && order.Package.Service?.CreatedBy != null
                        ? new AccountLiteModel
                        {
                            FirstName = order.Package.Service.CreatedBy.FirstName ?? string.Empty,
                            LastName = order.Package.Service.CreatedBy.LastName ?? string.Empty,
                            Username = order.Package.Service.CreatedBy.Username ?? string.Empty,
                            Email = order.Package.Service.CreatedBy.Email ?? string.Empty,
                            Image = order.Package.Service.CreatedBy.Image ?? string.Empty
                        }
                        : order.Package.OfferId.HasValue && order.Package.Offer?.CreatedBy != null
                            ? new AccountLiteModel
                            {
                                FirstName = order.Package.Offer.CreatedBy.FirstName ?? string.Empty,
                                LastName = order.Package.Offer.CreatedBy.LastName ?? string.Empty,
                                Username = order.Package.Offer.CreatedBy.Username ?? string.Empty,
                                Email = order.Package.Offer.CreatedBy.Email ?? string.Empty,
                                Image = order.Package.Offer.CreatedBy.Image ?? string.Empty
                            }
                            : null,
                    ServiceModel = order.Package?.Service == null ? null : new ServiceModel
                    {
                        Id = order.Package.Service.Id,
                        Name = order.Package.Service.Name ?? "Unknown",
                        Description = order.Package.Service.Description ?? string.Empty,
                        Status = order.Package.Service.Status,
                        CategoryId = order.Package.Service.CategoryId,
                        Rate = order.Package.Service.Rate ?? 0,
                        FeedbackCount = order.Package.Service.FeedbackCount ?? 0,
                        Price = order.Package.Price ?? 0,
                        MinWeight = order.Package.Service.MinWeight ?? 0,
                        MaxWeight = order.Package.Service.MaxWeight ?? 0,
                        ServiceAttachments = order.Package.Service.ServiceAttachments?.Select(att => new ServiceAttachment
                        {
                            Id = att.Id,
                            AttachmentUrl = att.AttachmentUrl ?? string.Empty,
                            AttachmentAlt = att.AttachmentAlt ?? string.Empty,
                            ServiceId = att.ServiceId
                        }).ToList() ?? new List<ServiceAttachment>()
                    },
                    OfferModel = order.Package?.Offer == null ? null : new OfferModel
                    {
                        Id = order.Package.Offer.Id,
                        Message = order.Package.Offer.Message ?? string.Empty,
                        Status = order.Package.Offer.Status,
                        MinWeight = order.Package.Offer.MinWeight ?? 0,
                        MaxWeight = order.Package.Offer.MaxWeight ?? 0,
                        RequestId = order.Package.Offer.RequestId ?? Guid.Empty,
                        ServiceId = order.Package.Offer.ServiceId ?? Guid.Empty,
                        OfferAttachments = order.Package.Offer.OfferAttachments?.Select(att => new OfferAttachment
                        {
                            Id = att.Id,
                            AttachmentUrl = att.AttachmentUrl ?? string.Empty,
                            AttachmentAlt = att.AttachmentAlt ?? string.Empty,
                            OfferId = att.OfferId
                        }).ToList() ?? new List<OfferAttachment>(),
                        CreatedBy = order.Package.Offer.CreatedBy != null
                            ? new AccountLiteModel
                            {
                                FirstName = order.Package.Offer.CreatedBy.FirstName ?? string.Empty,
                                LastName = order.Package.Offer.CreatedBy.LastName ?? string.Empty,
                                Username = order.Package.Offer.CreatedBy.Username ?? string.Empty,
                                Email = order.Package.Offer.CreatedBy.Email ?? string.Empty,
                                Image = order.Package.Offer.CreatedBy.Image ?? string.Empty
                            }
                            : null,
                        Package = order.Package != null
                            ? new PackageModel
                            {
                                Id = order.Package.Id,
                                Name = order.Package.Name,
                                Description = order.Package.Description,
                                DeliveryTime = order.Package.DeliveryTime,
                                SketchRevision = order.Package.SketchRevision,
                                ResponseTime = order.Package.ResponseTime != null
                                    ? TimeSpan.FromHours(order.Package.ResponseTime)
                                    : TimeSpan.Zero,
                                MaxQuantity = order.Package.MaxQuantity,
                                MinQuantity = order.Package.MinQuantity,
                                Price = order.Package.Price ?? 0,
                                ServiceId = order.Package.ServiceId,
                                OfferId = order.Package.OfferId
                            }
                            : null
                    }
                }).ToList();

                var result = new Pagination<OrderModel>(
                    orderModels,
                    orderFilterModel.PageIndex,
                    orderFilterModel.PageSize,
                    orders.TotalCount
                );

                return new ResponseModel
                {
                    Message = "Get all orders successfully",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while retrieving orders"
                };
            }
        }
        public async Task<ResponseModel> GetAllByAdmin(OrderFilterModel orderFilterModel)
        {

            var orders = await _unitOfWork.OrderRepository.GetAllAsync(
                filter: _ =>
                (orderFilterModel.IsDeleted == _.IsDeleted) &&
                (!orderFilterModel.Status.HasValue || _.Status == orderFilterModel.Status),
                include: _ => _.Include(_ => _.Package),
                order: _ =>
                {
                    switch (orderFilterModel.Order.ToLower())
                    {
                        case "recentdays":
                            return orderFilterModel.OrderByDescending
                                ? _.OrderByDescending(account => account.CreationDate)
                                : _.OrderBy(account => account.CreationDate);
                        case "olddays":
                            return orderFilterModel.OrderByDescending
                                ? _.OrderBy(account => account.CreationDate)
                                : _.OrderByDescending(account => account.CreationDate);
                        default:
                            return orderFilterModel.OrderByDescending
                                 ? _.OrderByDescending(account => account.CreationDate)
                                 : _.OrderBy(account => account.CreationDate);
                    }
                },
                pageIndex: orderFilterModel.PageIndex,
                pageSize: orderFilterModel.PageSize
                );
            var orderModels = orders.Data.Select(_ => new OrderModel
            {
                Id = _.Id,
                Phone = _.Phone,
                Address = _.Address,
                ToDistrict = _.ToDistrict,
                ToProvince = _.ToProvince,
                ToWard = _.ToWard,
                OrderStage = _.Stage,
                TotalPrice = _.TotalPrice,
                PackagePrice = _.OriginPrice,
                PackageName = _.Package.Name,
                Quantity = _.Quantity,
                ShipmentCode = _.ShipmentCode,
                Status = _.Status,

            }).ToList();
            var result = new Pagination<OrderModel>(orderModels, orderFilterModel.PageIndex,
                        orderFilterModel.PageSize, orders.Data.Count);

            return new ResponseModel
            {
                Message = "Get all orders successfully",
                Data = result
            };

        }
        public async Task<ResponseModel> UpdateStatus(Guid orderId, OrderStatus? orderStatus)
        {
            try
            {
                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized"
                    };
                }
                var order = await _unitOfWork.OrderRepository.GetAsync(orderId, include: _ => _.Include(_ => _.Package.Service.CreatedBy).ThenInclude(_ => _.Wallet).Include(_ => _.CreatedBy).ThenInclude(_ => _.Wallet).Include(_ => _.VoucherUsageLogs).ThenInclude(_ => _.Voucher).Include(_ => _.Transactions));
                if (order == null)
                {
                    return new ResponseModel
                    {
                        Message = "Not found",
                        Code = StatusCodes.Status404NotFound
                    };
                }
                if (order.Package.Service.CreatedBy.Id != currentUserId.Value)
                {
                    return new ResponseModel
                    {
                        Message = "You do not have permission to cancel this order",
                        Code = StatusCodes.Status403Forbidden
                    };
                }
                order.Status = orderStatus ?? throw new ArgumentNullException(nameof(orderStatus), "Order status cannot be null");

                switch (orderStatus)
                {
                    case OrderStatus.Accepted:
                        order.StartTime = DateTime.UtcNow;
                        order.Stage = OrderStage.SketchInProcess;
                        break;

                    case OrderStatus.Rejected:
                        if (order.CreatedBy?.Wallet == null)
                        {
                            return new ResponseModel
                            {
                                Code = StatusCodes.Status404NotFound,
                                Message = "Wallet not found"
                            };
                        }

                        var refundTransaction = new Transaction
                        {
                            Amount = order.TotalPrice,
                            Type = TransactionType.TransferIn,
                            CreatedById = currentUserId.Value,
                            Status = TransactionStatus.Completed,
                            WalletId = order.CreatedBy.Wallet.Id
                        };

                        foreach (var voucherLog in order.VoucherUsageLogs)
                        {
                            if (voucherLog.Voucher.TotalQuantity.HasValue && voucherLog.Voucher.RemainingQuantity.HasValue)
                            {
                                voucherLog.UsageStatus = UsageStatus.Refunded;
                                voucherLog.Voucher.RemainingQuantity += 1;
                                if (voucherLog.Voucher.VoucherStatus == VoucherStatus.OutOfStock)
                                {
                                    voucherLog.Voucher.VoucherStatus = VoucherStatus.Pending;
                                }
                            }
                            //voucherLog.UsageStatus = UsageStatus.Cancelled;
                        }

                        order.CreatedBy.Wallet.Balance += (decimal)order.TotalPrice;

                        order.Transactions.Add(refundTransaction);
                        order.Status = OrderStatus.Cancelled;
                        order.Stage = OrderStage.Cancelled;
                        break;

                    default:
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = "Invalid order status"
                        };
                }
                _unitOfWork.OrderRepository.Update(order);
                var result = await _unitOfWork.SaveChangeAsync();

                if (result > 0)
                {
                    if(orderStatus == OrderStatus.Rejected)
                    {
                        var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Customer_RejectOrder).Result;
                        if (notificationContent != null)
                        {
                            var notificationAddModel = new NotificationAddModel
                            {
                                Content = notificationContent.Content.Replace("[#orderCode]", order.Code),
                                AccountId = (Guid)(order.CreatedById),
                                NotificationContentId = notificationContent.Id,
                                SourceId = order.Id
                            };
                            await _notificationService.PushNotification(notificationAddModel);
                        }
                    }
                    if (orderStatus == OrderStatus.Accepted) 
                    {
                        var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Customer_AcceptOrder).Result;
                        if (notificationContent != null)
                        {
                            var notificationAddModel = new NotificationAddModel
                            {
                                Content = notificationContent.Content.Replace("[#orderCode]", order.Code),
                                AccountId = (Guid)(order.CreatedById),
                                NotificationContentId = notificationContent.Id,
                                SourceId = order.Id
                            };
                            await _notificationService.PushNotification(notificationAddModel);
                        }
                    }
                }

                return result > 0
                    ? new ResponseModel { Message = "Successfully" }
                    : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Fail" };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<ResponseModel> UsedAdminVoucher(Guid orderId, Guid voucherId)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }
            var order = await _unitOfWork.OrderRepository.GetAsync(orderId);
            if (order == null)
            {
                return new ResponseModel
                {
                    Message = "Order not found.",
                    Code = StatusCodes.Status404NotFound
                };
            }

            if (order.Status != OrderStatus.Accepted)
            {
                return new ResponseModel
                {
                    Message = "Voucher can only be applied to orders in success status.",
                    Code = StatusCodes.Status400BadRequest
                };
            }

            var voucher = await _unitOfWork.VoucherRepository.GetAsync(voucherId);
            if (voucher == null)
            {
                return new ResponseModel
                {
                    Message = "Voucher not found.",
                    Code = StatusCodes.Status404NotFound
                };
            }

            if (voucher.TotalQuantity.HasValue && voucher.RemainingQuantity < 1 && voucher.VoucherStatus == VoucherStatus.OutOfStock)
            {
                return new ResponseModel
                {
                    Message = "Voucher is out of stock.",
                    Code = StatusCodes.Status400BadRequest
                };
            }
            var hasUsed = await _unitOfWork.VoucherUsageLogRepository.CheckOrderHasUsedVoucher(orderId, currentUserId.Value);
            if (hasUsed)
            {
                return new ResponseModel
                {
                    Message = "One order just have one voucher.",
                    Code = StatusCodes.Status400BadRequest
                };
            }
            var totalPriceOrder = order.TotalPrice - order.ShippingPrice ?? 0;

            var voucherDiscountValue = voucher?.DiscountValue ?? 0;
            var adminCommAfterUsed = await AdminCommission(totalPriceOrder, voucherDiscountValue);

            if (voucher?.MaxDiscountValue != null && adminCommAfterUsed > voucher.MaxDiscountValue.Value)
            {
                adminCommAfterUsed = voucher.MaxDiscountValue.Value;
            }

            order.AdminCommUsedVch = adminCommAfterUsed;
            order.ArtistRevenue = totalPriceOrder - adminCommAfterUsed;

            order.VoucherUsageLogs.Add(new VoucherUsageLog
            {
                VoucherId = voucher.Id,
                CreatedById = (Guid)order.CreatedById,
                DiscountValue = (decimal)(order.AdminCommDefault - adminCommAfterUsed),
                DiscountValueOrigin = voucherDiscountValue,
                UsageStatus = UsageStatus.Used
            });
            if (voucher.TotalQuantity.HasValue && voucher.RemainingQuantity >= 1)
            {
                voucher.RemainingQuantity -= 1;
                if (voucher.RemainingQuantity == 0)
                {
                    voucher.VoucherStatus = VoucherStatus.OutOfStock;
                }
            }

            _unitOfWork.OrderRepository.Update(order);
            _unitOfWork.VoucherRepository.Update(voucher);

            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0
                ? new ResponseModel
                {
                    Message = "Voucher applied successfully.",
                    Code = StatusCodes.Status200OK
                }
                : new ResponseModel
                {
                    Message = "Failed to apply voucher.",
                    Code = StatusCodes.Status400BadRequest
                };
        }
        public async Task<ResponseModel> Cancel(Guid orderId, Guid? cancellationReasonId)
        {
            try
            {
                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized"
                    };
                }

                var account = await _unitOfWork.AccountRepository.GetAsync(
                    currentUserId.Value, include: _ => _.Include(_ => _.Wallet));

                if (account?.Wallet == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Wallet not found."
                    };
                }

                var order = await _unitOfWork.OrderRepository.GetAsync(
                    orderId, include: _ => _.Include(_ => _.OrderTrackings)

                                            .Include(_ => _.Package.Service.CreatedBy.AccountRoles).ThenInclude(_ => _.Role)
                                            .Include(_ => _.CreatedBy.AccountRoles).ThenInclude(_ => _.Role));

                if (order == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found."
                    };
                }
                if (order.Status == OrderStatus.Cancelled && order.Stage == OrderStage.Cancelled)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Order has cancelled."
                    };
                }
                var cancellationReason = await _unitOfWork.CancellationReasonRepository.GetAsync((Guid)cancellationReasonId);

                if (cancellationReason == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Cancellation reason not found."
                    };
                }

                bool appliesToCustomer = cancellationReason.RoleType.Equals(Chillde.Repositories.Enums.Role.Customer);
                bool appliesToArtisan = cancellationReason.RoleType.Equals(Chillde.Repositories.Enums.Role.Artisan);

                var accountId = order.Package.CreatedById;
                var findAccount = await _unitOfWork.AccountRepository.GetAsync((Guid)accountId, include: _ => _.Include(_ => _.AccountRoles).ThenInclude(_ => _.Role));

                var accountRoleArtisan = findAccount?.AccountRoles
                    .FirstOrDefault(_ => appliesToArtisan && _.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString());

                var accountRoleCustomer = order.CreatedBy?.AccountRoles
                    .FirstOrDefault(_ => appliesToCustomer && _.Role.Name == Chillde.Repositories.Enums.Role.Customer.ToString());

                if (appliesToArtisan && accountRoleArtisan != null)
                {
                    accountRoleArtisan.TotalReputation -= cancellationReason.Value;
                    var reputationLog = new ReputationLog
                    {
                        PointChange = -cancellationReason.Value,
                        Reason = cancellationReason.Name,
                        OrderId = order.Id,
                    };
                    accountRoleArtisan.Reputations.Add(reputationLog);
                    _unitOfWork.AccountRoleRepository.Update(accountRoleArtisan);
                }
                if (appliesToCustomer && accountRoleCustomer != null)
                {
                    accountRoleCustomer.TotalReputation -= cancellationReason.Value;
                    var reputationLog = new ReputationLog
                    {
                        PointChange = -cancellationReason.Value,
                        Reason = cancellationReason.Name,
                        OrderId = order.Id,
                    };
                    accountRoleCustomer.Reputations.Add(reputationLog);
                    _unitOfWork.AccountRoleRepository.Update(accountRoleCustomer);
                }

                var transaction = new Transaction
                {
                    Amount = order.TotalPrice,
                    Type = TransactionType.TransferIn,
                    CreatedById = currentUserId.Value,
                    Status = TransactionStatus.Completed,
                    WalletId = account.Wallet.Id
                };

                account.Wallet.Balance += (decimal)order.TotalPrice;
                order.Transactions.Add(transaction);
                order.Status = OrderStatus.Cancelled;
                order.Stage = OrderStage.Cancelled;
                order.CancellationReason = cancellationReason;

                _unitOfWork.OrderRepository.Update(order);
                _unitOfWork.AccountRepository.Update(account);

                var result = await _unitOfWork.SaveChangeAsync();

                return result > 0
                    ? new ResponseModel { Message = "Cancel order successfully" }
                    : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Cancel order unsuccessfully" };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<ResponseModel> GetAllOrderTrackings(Guid orderId, OrderStage? orderStage)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            var order = await _unitOfWork.OrderRepository.GetAsync(
                orderId, include: _ => _.Include(_ => _.OrderTrackings)
                                         .ThenInclude(_ => _.CreatedBy)
                                         .ThenInclude(_ => _.AccountRoles)
                                         .Include(_ => _.OrderTrackings)
                                         .ThenInclude(_ => _.OrderTrackingAttachments)
                                         .Include(_ => _.Package)
                                         .ThenInclude(_ => _.Service));

            if (order == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order not found."
                };
            }
            var customerId = order.CreatedById;
            var filteredTrackings = order.OrderTrackings
                .Where(_ => !orderStage.HasValue || _.Stage == orderStage)
                .Select(_ => new OrderTrackingModel
                {
                    Id = _.Id,
                    CreatedBy = $"{_.CreatedBy.FirstName} {_.CreatedBy.LastName}",
                    CreatedById = _.CreatedById,
                    DeletedById = _.CreatedById,
                    CreatedRole = _.CreatedById == customerId ? Repositories.Enums.Role.Customer : Repositories.Enums.Role.Artisan,
                    CurrentSketchRevision = order.CurrentSketchRevision,
                    Name = _.Name,
                    Description = _.Description,
                    IsAccepted = _.IsAccepted,
                    Stage = _.Stage,
                    Type = _.Type,
                    CreationDate = _.CreationDate,
                    OrderTrackingAttachmentModels = _.OrderTrackingAttachments?
                        .Select(_ => new OrderTrackingAttachmentModel
                        {
                            Id = _.Id,
                            AttachmentUrl = _.AttachmentUrl,
                            AttachmentAlt = _.AttachmentAlt
                        }).ToList()
                })
                .ToList();

            return new ResponseModel
            {
                Data = filteredTrackings.Any() ? filteredTrackings : null,
                Message = "Order trackings retrieved successfully."
            };
        }
        public async Task<ResponseModel> AddSketch(Guid orderId, OrderTrackingAddModel orderTrackingAddModel)
        {
            try
            {
                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized"
                    };
                }

                var order = await _unitOfWork.OrderRepository.GetAsync(orderId, include: _ => _
                    .Include(_ => _.OrderTrackings)
                    .Include(_ => _.CreatedBy)
                    .Include(_ => _.Package));

                if (order == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found."
                    };
                }

                if (orderTrackingAddModel == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid tracking data."
                    };
                }


                if (order.Stage != OrderStage.SketchInProcess && order.Stage != OrderStage.ReviewSketch)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Error in stage."
                    };
                }
                var latestSketchs = order.OrderTrackings.OrderByDescending(_ => _.CreationDate).ToList();
                var roles = await _unitOfWork.AccountRoleRepository.GetAllAsync(filter: _ => _.AccountId == currentUserId.Value, include: _ => _.Include(_ => _.Role));

                if (roles.Data.Count() == 1)
                {
                    if (order.CurrentSketchRevision <= 0)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = "Sketch tracking cannot be added. No revisions available."
                        };
                    }

                    OrderTracking lastSketch = null;

                    foreach (var tracking in latestSketchs)
                    {
                        if (tracking.Type == OrderTrackingType.Sketch && tracking.IsAccepted == null)
                        {
                            lastSketch = tracking;
                            break;
                        }
                    }
                    if (lastSketch != null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = "Cannot add new sketch. Please accept or reject the previous sketch first."
                        };
                    }
                }

                if (order.Stage == OrderStage.SketchInProcess)
                {
                    order.Stage = OrderStage.ReviewSketch;
                }

                var uploadedAttachments = await UploadAttachments(
                    orderTrackingAddModel.Attachments,
                    order.Code,
                    FolderAttachment.TRACKINGSKETCH
                );

                var orderTracking = new OrderTracking
                {
                    // Name = orderTrackingAddModel.Name ?? "New Sketch",
                    // Description = orderTrackingAddModel.Description ?? "Sketch phase",
                    Name = orderTrackingAddModel.Name,
                    Description = orderTrackingAddModel.Description,
                    Type = orderTrackingAddModel.Type,
                    Stage = OrderStage.ReviewSketch,
                    CreatedById = currentUserId.Value,
                    OrderTrackingAttachments = uploadedAttachments
                };
                if (orderTracking.Type == OrderTrackingType.Sketch)
                {
                    orderTracking.IsDeadlineSent = false;
                    orderTracking.IsReminder50Sent = false;
                    orderTracking.IsReminder80Sent = false;
                }
                order.OrderTrackings.Add(orderTracking);
                _unitOfWork.OrderRepository.Update(order);

                int result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    if (orderTracking.Type == OrderTrackingType.Sketch)
                    {
                        await SendNew(order.CreatedBy.Email, order.Code, "sketch", order);
                    }
                    var createdBy = await _unitOfWork.AccountRepository.GetAsync((Guid)orderTracking.CreatedById);

                    var trackingModel = new OrderTrackingModel
                    {
                        Id = orderTracking.Id,
                        CreatedBy = $"{createdBy?.FirstName} {createdBy?.LastName}",
                        CreatedById = orderTracking.CreatedById,
                        DeletedById = orderTracking.CreatedById,
                        CreatedRole = order.CreatedById == orderTracking.CreatedById
                            ? Repositories.Enums.Role.Customer
                            : Repositories.Enums.Role.Artisan,
                        CurrentSketchRevision = order.CurrentSketchRevision,
                        Name = orderTracking.Name,
                        Description = orderTracking.Description,
                        IsAccepted = orderTracking.IsAccepted,
                        Stage = orderTracking.Stage,
                        Type = orderTracking.Type,
                        CreationDate = orderTracking.CreationDate,
                        OrderTrackingAttachmentModels = orderTracking.OrderTrackingAttachments?
                            .Select(_ => new OrderTrackingAttachmentModel
                            {
                                Id = _.Id,
                                AttachmentUrl = _.AttachmentUrl,
                                AttachmentAlt = _.AttachmentAlt
                            }).ToList()
                    };

                    var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Customer_NewSketch).Result;
                    if (notificationContent != null)
                    {
                        var notificationAddModel = new NotificationAddModel
                        {
                            Content = notificationContent.Content.Replace("[#orderCode]", order.Code),
                            AccountId = (Guid)(order.Package.CreatedById),
                            NotificationContentId = notificationContent.Id,
                            SourceId = order.Id
                        };
                        await _notificationService.PushNotification(notificationAddModel);
                    }

                    return new ResponseModel
                    {
                        Data = trackingModel,
                        Code = StatusCodes.Status200OK,
                        Message = "Sketch tracking added"
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Failed to add sketch tracking."
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<OrderTrackingAttachment>> UploadAttachments(IEnumerable<AttachmentAddModel> attachments, string orderCode, string folderName)
        {
            var uploadedAttachments = new List<OrderTrackingAttachment>();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    // var attachmentPath = await _cloudinaryHelper.UploadImageAsync(
                    //     attachment.AttachmentUrl,
                    //     orderCode,
                    //     folderName: FolderAttachment.TRACKINGSKETCH
                    // );

                    uploadedAttachments.Add(new OrderTrackingAttachment
                    {
                        AttachmentUrl = attachment.AttachmentUrl,
                        AttachmentAlt = attachment.AttachmentAlt
                    });
                }
            }

            return uploadedAttachments;
        }
        public async Task SendNew(string email, string orderCode, string title, Repositories.Entities.Order order)
        {
            title = title.ToLower().Trim();

            string subject, body;

            if (title == "sketch")
            {
                subject = $"📢 New Sketch Uploaded for Order #{orderCode}";
                body = $@"
                         <p>Dear {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                         <p>Good news! A new sketch has been uploaded by the artisan for your order <strong>#{orderCode}</strong>.</p>
                         <p><strong>Action Required:</strong></p>
                         <ul>
                             <li>Review the sketch.</li>
                             <li>Approve it or request revisions.</li>
                             <li>Ensure timely responses to avoid delays.</li>
                         </ul>
                         <p>Best regards,</p>
                         <p><strong>From Chillde</strong></p>";
            }
            else if (title == "delivery")
            {
                subject = $"📦 Your Order #{orderCode} Has Been Delivered!";
                body = $@"
                          <p>Dear {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                          <p>Exciting update! Your artisan has completed and delivered the final product for order <strong>#{orderCode}</strong>.</p>
                          <p><strong>Next Steps:</strong></p>
                          <ul>
                              <li>Review your delivered product.</li>
                              <li>Confirm delivery or request adjustments before the artist must close the order and send it for shipping.</li>
                          </ul>
                          <p>Thank you for choosing us!</p>
                          <p><strong>From Chillde</strong></p>";
            }
            else
            {
                return;
            }

            await _iIEmailHelper.SendEmailAsync(email, subject, body, true);
        }
        public async Task<ResponseModel> AddDelivery(Guid orderId, OrderTrackingAddModel orderTrackingAddModel)
        {
            try
            {
                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized"
                    };
                }

                var order = await _unitOfWork.OrderRepository.GetAsync(orderId, include: _ => _
                    .Include(_ => _.OrderTrackings)
                    .Include(_ => _.CreatedBy));

                if (order == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found."
                    };
                }

                if (orderTrackingAddModel == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid tracking data."
                    };
                }
                if (order.StartTime.HasValue && order.DeliveryTime.HasValue)
                {
                    DateTime expectedDeliveryDate = order.StartTime.Value.AddDays(order.DeliveryTime.Value);
                    if (expectedDeliveryDate <= DateTime.UtcNow)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = "Cannot add delivery. Expected delivery time has not passed yet."
                        };
                    }
                }
                if (order.Stage != OrderStage.DeliveryInProcess && order.Stage != OrderStage.ReviewDelivery)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Error in stage."
                    };
                }
                var roles = await _unitOfWork.AccountRoleRepository.GetAllAsync(filter: _ => _.AccountId == currentUserId.Value, include: _ => _.Include(_ => _.Role));

                if (order.Stage == OrderStage.DeliveryInProcess)
                {
                    order.Stage = OrderStage.ReviewDelivery;
                }

                var uploadedAttachments = await UploadAttachments(
                    orderTrackingAddModel.Attachments,
                    order.Code,
                    FolderAttachment.TRACKINGDELIVERY
                );

                var orderTracking = new OrderTracking
                {
                    // Name = orderTrackingAddModel.Name ?? "New Delivery",
                    // Description = orderTrackingAddModel.Description ?? "Delivery phase",
                    Name = orderTrackingAddModel.Name,
                    Description = orderTrackingAddModel.Description,
                    Type = orderTrackingAddModel.Type,
                    Stage = OrderStage.ReviewDelivery,
                    CreatedById = currentUserId.Value,
                    OrderTrackingAttachments = uploadedAttachments
                };
                order.OrderTrackings.Add(orderTracking);
                _unitOfWork.OrderRepository.Update(order);

                int result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    if (orderTracking.Type == OrderTrackingType.Delivery)
                    {
                        await SendNew(order.CreatedBy.Email, order.Code, "delivery", order);
                    }
                    var createdBy = await _unitOfWork.AccountRepository.GetAsync((Guid)orderTracking.CreatedById);

                    var trackingModel = new OrderTrackingModel
                    {
                        Id = orderTracking.Id,
                        CreatedBy = $"{createdBy?.FirstName} {createdBy?.LastName}",
                        CreatedById = orderTracking.CreatedById,
                        DeletedById = orderTracking.CreatedById,
                        CreatedRole = order.CreatedById == orderTracking.CreatedById
                            ? Repositories.Enums.Role.Customer
                            : Repositories.Enums.Role.Artisan,
                        CurrentSketchRevision = order.CurrentSketchRevision,
                        Name = orderTracking.Name,
                        Description = orderTracking.Description,
                        IsAccepted = orderTracking.IsAccepted,
                        Stage = orderTracking.Stage,
                        Type = orderTracking.Type,
                        CreationDate = orderTracking.CreationDate,
                        OrderTrackingAttachmentModels = orderTracking.OrderTrackingAttachments?
                            .Select(_ => new OrderTrackingAttachmentModel
                            {
                                Id = _.Id,
                                AttachmentUrl = _.AttachmentUrl,
                                AttachmentAlt = _.AttachmentAlt
                            }).ToList()
                    };

                    var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Customer_NewDelivery).Result;
                    if (notificationContent != null)
                    {
                        var notificationAddModel = new NotificationAddModel
                        {
                            Content = notificationContent.Content.Replace("[#orderCode]", order.Code),
                            AccountId = (Guid)(order.Package.CreatedById),
                            NotificationContentId = notificationContent.Id,
                            SourceId = order.Id
                        };
                        await _notificationService.PushNotification(notificationAddModel);
                    }

                    return new ResponseModel
                    {
                        Data = trackingModel,
                        Code = StatusCodes.Status200OK,
                        Message = "Delivery tracking added"
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Failed to add delivery tracking."
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ResponseModel> GetOrderDetail(Guid orderId)
        {
            try
            {
                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized"
                    };
                }

                var order = await _unitOfWork.OrderRepository.GetAsync(
                       orderId,
                       include: _ => _
                           .Include(_ => _.CancellationReason)
                           .Include(_ => _.CreatedBy)
                           .Include(_ => _.Package.PackageFeatures).ThenInclude(_ => _.Feature)
                           .Include(_ => _.Package.Offer).ThenInclude(_ => _.OfferAttachments)
                           .Include(_ => _.Package.Offer).ThenInclude(_ => _.CreatedBy)
                           .Include(_ => _.Package.Service).ThenInclude(_ => _.CreatedBy)
                           .Include(_ => _.Package.Service).ThenInclude(_ => _.ServiceAttachments) // For Attachments
                           .Include(_ => _.VoucherUsageLogs).ThenInclude(_ => _.Voucher)
                           .Include(_ => _.OrderInformations).ThenInclude(_ => _.OrderInformationAttachments)
                           .Include(_ => _.OrderInformations).ThenInclude(_ => _.PackageFeature).ThenInclude(_ => _.Feature)
                           .Include(_ => _.Shipments)
                   );
                if (order == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found"
                    };
                }

                var model = new OrderDetailModel
                {
                    Id = order?.Id ?? Guid.Empty,
                    Name = order.Package.ServiceId.HasValue && order.Package.Service != null
                        ? order.Package.Service.Name ?? "Unknown"
                        : order.Package.OfferId.HasValue && order.Package.Offer != null
                            ? order.Package.Offer.Message ?? "Unknown"
                            : "Unknown",
                    ShipmentId = order.Shipments?.FirstOrDefault()?.Id.ToString() ?? string.Empty,
                    Attachments = order.Package.ServiceId.HasValue && order.Package.Service != null
                        ? order.Package.Service.ServiceAttachments?.Select(att => new OrderAttachmentModel
                        {
                            Id = att.Id,
                            AttachmentUrl = att.AttachmentUrl ?? string.Empty,
                            AttachmentAlt = att.AttachmentAlt ?? string.Empty
                        }).ToList() ?? new List<OrderAttachmentModel>()
                        : order.Package.OfferId.HasValue && order.Package.Offer != null
                            ? order.Package.Offer.OfferAttachments?.Select(att => new OrderAttachmentModel
                            {
                                Id = att.Id,
                                AttachmentUrl = att.AttachmentUrl ?? string.Empty,
                                AttachmentAlt = att.AttachmentAlt ?? string.Empty
                            }).ToList() ?? new List<OrderAttachmentModel>()
                            : new List<OrderAttachmentModel>(),
                    CreatedById = order?.CreatedById ?? Guid.Empty,
                    Code = order?.Code ?? string.Empty,
                    Phone = order.Phone ?? "",
                    Address = order.Address ?? "",
                    Ward = order?.ToWard ?? "N/A",
                    District = order?.ToDistrict ?? "N/A",
                    Province = order?.ToProvince ?? "N/A",
                    Quantity = order?.Quantity ?? 1,
                    ShipmentCode = order?.ShipmentCode ?? string.Empty,
                    DeliveryTime = order?.DeliveryTime,
                    StartTime = order?.StartTime,
                    Deadline = order?.StartTime.HasValue == true && order?.DeliveryTime.HasValue == true
                    ? order.StartTime.Value.AddDays(order.DeliveryTime.Value)
                    : null,
                    Stage = order?.Stage ?? 0,
                    Status = order?.Status ?? 0,
                    OriginPrice = order?.OriginPrice ?? 0,
                    TotalPrice = order?.TotalPrice ?? 0,
                    ShippingPrice = order?.ShippingPrice ?? 0,
                    AfterApplyVoucherPrice = order?.AfterApplyVoucherPrice ?? 0,
                    ArtistRevenueAfterCancel = order?.ArtistRevenueAfterCancel ?? 0,
                    AdminCommUsedVch = order?.AdminCommUsedVch ?? 0,
                    AdminCommDefault = order?.AdminCommDefault ?? 0,
                    ArtistRevenue = order?.ArtistRevenue ?? 0,
                    VoucherCost = order?.VoucherCost ?? 0,
                    CurrentSketchRevision = order?.CurrentSketchRevision ?? 0,
                    CancelOrderReason = order?.CancellationReason?.Name ?? "N/A",
                    AutoCancelOrderReason = order?.SystemCancelReason?.ToString() ?? "N/A",
                    CreationDate = order?.CreationDate ?? DateTime.MinValue,
                    VoucherUsages = order?.VoucherUsageLogs?.Select(_ => new VoucherUsageModel
                    {
                        Id = _.Id,
                        VoucherId = _.Voucher?.Id ?? Guid.Empty,
                        VoucherCode = _.Voucher?.Code ?? "N/A",
                        DiscountValue = _.DiscountValue,
                        DiscountOriginalValue = _.DiscountValueOrigin,
                        UsageStatus = _.UsageStatus,
                        CreationDate = _.CreationDate,
                    }).ToList() ?? new List<VoucherUsageModel>(),


                    OrderInformation = order?.OrderInformations?.Select(_ => new OrderInformationModel
                    {
                        Id = _.Id,
                        FeatureId = _.PackageFeature?.Feature?.Id ?? Guid.Empty,
                        FeatureName = _.PackageFeature?.Feature?.Name ?? string.Empty,
                        Description = _.Description ?? string.Empty,
                        IsExtra = _.PackageFeature?.IsExtra ?? false,
                        Quantity = _.Quantity ?? 0,
                        Price = _.Price ?? 0,
                        Attachments = _.OrderInformationAttachments?.Select(att => new OrderAttachmentModel
                        {
                            Id = att.Id,
                            AttachmentUrl = att.AttachmentUrl ?? string.Empty,
                            AttachmentAlt = att.AttachmentAlt ?? string.Empty
                        }).ToList() ?? new List<OrderAttachmentModel>()
                    }).ToList() ?? new List<OrderInformationModel>(),
                    Customer = order?.CreatedBy != null
                    ? new AccountLiteModel
                    {
                        FirstName = order.CreatedBy.FirstName ?? string.Empty,
                        LastName = order.CreatedBy.LastName ?? string.Empty,
                        Username = order.CreatedBy.Username ?? string.Empty,
                        Email = order.CreatedBy.Email ?? string.Empty,
                        Image = order.CreatedBy.Image ?? string.Empty,
                    }
                    : null,
                    Artisan = order.Package.ServiceId.HasValue && order.Package.Service?.CreatedBy != null
                    ? new AccountLiteModel
                    {
                        FirstName = order.Package.Service.CreatedBy.FirstName ?? string.Empty,
                        LastName = order.Package.Service.CreatedBy.LastName ?? string.Empty,
                        Username = order.Package.Service.CreatedBy.Username ?? string.Empty,
                        Email = order.Package.Service.CreatedBy.Email ?? string.Empty,
                        Image = order.Package.Service.CreatedBy.Image ?? string.Empty
                    }
                    : order.Package.OfferId.HasValue && order.Package.Offer?.CreatedBy != null
                    ? new AccountLiteModel
                    {
                        FirstName = order.Package.Offer.CreatedBy.FirstName ?? string.Empty,
                        LastName = order.Package.Offer.CreatedBy.LastName ?? string.Empty,
                        Username = order.Package.Offer.CreatedBy.Username ?? string.Empty,
                        Email = order.Package.Offer.CreatedBy.Email ?? string.Empty,
                        Image = order.Package.Offer.CreatedBy.Image ?? string.Empty
                    }
                    : null
                };

                if (order.Package?.Offer != null)
                {
                    model.OfferModel = new OfferModel
                    {
                        Id = order.Package.Offer.Id,
                        Message = order.Package.Offer.Message ?? string.Empty,
                        Status = order.Package.Offer.Status,
                        MinWeight = order.Package.Offer.MinWeight ?? 0,
                        MaxWeight = order.Package.Offer.MaxWeight ?? 0,
                        RequestId = order.Package.Offer.RequestId ?? Guid.Empty,
                        ServiceId = order.Package.Offer.ServiceId ?? Guid.Empty,
                        OfferAttachments = order.Package.Offer.OfferAttachments?.Select(attachment => new OfferAttachment
                        {
                            Id = attachment.Id,
                            AttachmentAlt = attachment.AttachmentAlt ?? string.Empty,
                            AttachmentUrl = attachment.AttachmentUrl ?? string.Empty,
                            OfferId = attachment.OfferId
                        }).ToList() ?? new List<OfferAttachment>(),
                        Package = order.Package.Offer.Package != null
                            ? new PackageModel
                            {
                                Id = order.Package.Offer.Package.Id,
                                Name = order.Package.Offer.Package.Name,
                                Description = order.Package.Offer.Package.Description,
                                DeliveryTime = order.Package.Offer.Package.DeliveryTime,
                                SketchRevision = order.Package.Offer.Package.SketchRevision,
                                ResponseTime = order.Package.Offer.Package.ResponseTime != null
                                ? TimeSpan.FromHours(order.Package.Offer.Package.ResponseTime)
                                : TimeSpan.Zero,
                                MaxQuantity = order.Package.Offer.Package.MaxQuantity,
                                Price = order.Package.Offer.Package.Price ?? 0,
                                PackageFeatures = order.Package.Offer.Package?.PackageFeatures?.Select(feature => new PackageFeatureModel
                                {
                                    Id = feature.Id,
                                    Name = feature.Name ?? string.Empty,
                                    AdditionalCost = feature.AdditionalCost ?? 0,
                                    AdditionalDay = feature.AdditionalDay ?? 0,
                                    IsExtra = feature.IsExtra ?? false,
                                    IsChecked = feature.IsChecked ?? false,
                                    MaxQuantity = feature.MaxQuantity ?? 0,
                                    PackageId = feature.PackageId ?? Guid.Empty,
                                    FeatureId = feature.FeatureId
                                }).ToList() ?? new List<PackageFeatureModel>(),
                                Features = order.Package.Offer.Package?.PackageFeatures.Select(_ => _.Feature).Select(feature => new FeatureModel
                                {
                                    Id = feature.Id,
                                    Name = feature.Name ?? string.Empty,
                                    IsInformationRequired = feature.IsInformationRequired ? true : false,
                                    Question = feature.Question ?? string.Empty,
                                    QuestionType = feature.QuestionType,
                                    IsQuantity = feature.IsQuantity ? true : false,
                                }).ToList() ?? new List<FeatureModel>()
                            }
                            : null,
                        CreatedBy = new AccountLiteModel
                        {
                            FirstName = order.Package.Offer.CreatedBy.FirstName,
                            LastName = order.Package.Offer.CreatedBy.LastName ?? string.Empty,
                            Username = order.Package.Offer.CreatedBy.Username ?? string.Empty,
                            Email = order.Package.Offer.CreatedBy.Email ?? string.Empty,
                            Image = order.Package.Offer.CreatedBy.Image ?? string.Empty,
                        }
                    };
                }
                else
                {
                    model.PackageModel = order.Package != null
                        ? new PackageModel
                        {
                            Id = order.Package.Id,
                            Name = order.Package.Name,
                            ServiceId = order.Package.ServiceId,
                            Description = order.Package.Description,
                            DeliveryTime = order.Package.DeliveryTime,
                            SketchRevision = order.Package.SketchRevision,
                            ResponseTime = order.Package.ResponseTime != null
                                ? TimeSpan.FromHours(order.Package.ResponseTime)
                                : TimeSpan.Zero,
                            MaxQuantity = order.Package.MaxQuantity,
                            Price = order.Package.Price ?? 0,
                            PackageFeatures = order.Package?.PackageFeatures?.Select(feature => new PackageFeatureModel
                            {
                                Id = feature.Id,
                                Name = feature.Name ?? string.Empty,
                                AdditionalCost = feature.AdditionalCost ?? 0,
                                AdditionalDay = feature.AdditionalDay ?? 0,
                                IsExtra = feature.IsExtra ?? false,
                                IsChecked = feature.IsChecked ?? false,
                                MaxQuantity = feature.MaxQuantity ?? 0,
                                PackageId = feature.PackageId ?? Guid.Empty,
                                FeatureId = feature.FeatureId
                            }).ToList() ?? new List<PackageFeatureModel>(),
                            Features = order.Package?.PackageFeatures.Select(_ => _.Feature).Select(feature => new FeatureModel
                            {
                                Id = feature.Id,
                                Name = feature.Name ?? string.Empty,
                                IsInformationRequired = feature.IsInformationRequired ? true : false,
                                Question = feature.Question ?? string.Empty,
                                QuestionType = feature.QuestionType,
                                IsQuantity = feature.IsQuantity ? true : false,
                            }).ToList() ?? new List<FeatureModel>()
                        }
                        : null;
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Get order detail successfully",
                    Data = model
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<ResponseModel> UpdateOrderAfterDeliveryAsync(Guid orderId)
        {
            try
            {
                if (orderId == Guid.Empty)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid order ID."
                    };
                }

                var order = await _unitOfWork.OrderRepository.GetAsync(
                    orderId,
                    include: o => o.Include(o => o.Package)
                );
                if (order == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found."
                    };
                }
                if (order.Stage != OrderStage.AwaitingClosure)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = $"Order is not in AwaitingClosure stage. Current stage: {order.Stage}."
                    };                }
                if (order.Status != OrderStatus.Accepted)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = $"Order is not in Accepted status. Current status: {order.Status}."
                    };
                }

                var artisanAccount = await _unitOfWork.AccountRepository.GetAsync(
                    (Guid)order.Package.CreatedById,
                    include: a => a.Include(a => a.Wallet).Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                );
                if (artisanAccount == null || artisanAccount.Wallet == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Artisan account or wallet not found."
                    };
                }

                var customerAccount = await _unitOfWork.AccountRepository.GetAsync(
                    (Guid)order.CreatedById,
                    include: a => a.Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                );
                if (customerAccount == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Customer account not found."
                    };
                }

                var accountRoleArtisan = artisanAccount.AccountRoles
                    .FirstOrDefault(ar => ar.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString());
                if (accountRoleArtisan == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Artisan role not found for account."
                    };
                }

                var accountRoleCustomer = customerAccount.AccountRoles
                    .FirstOrDefault(ar => ar.Role.Name == Chillde.Repositories.Enums.Role.Customer.ToString());
                if (accountRoleCustomer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Customer role not found for account."
                    };
                }

                 await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var wallet = artisanAccount.Wallet;
                    wallet.Balance += (decimal)order.ArtistRevenue;
                    _unitOfWork.WalletRepository.Update(wallet);

                    order.Transactions.Add(new Transaction
                    {
                        WalletId = wallet.Id,
                        Amount = order.ArtistRevenue,
                        Type = TransactionType.TransferIn,
                        Status = TransactionStatus.Completed,
                        CreatedById = artisanAccount.CreatedById
                    });

                    order.Stage = OrderStage.Completed;
                    order.Status = OrderStatus.Completed;
                    _unitOfWork.OrderRepository.Update(order);

                    if (accountRoleCustomer.TotalReputation < 100)
                    {
                        accountRoleCustomer.TotalReputation += 1;
                        _unitOfWork.AccountRoleRepository.Update(accountRoleCustomer);

                        var customerReputationLog = new ReputationLog
                        {
                            PointChange = +1,
                            Reason = "Đã hoàn thành đơn hàng với tư cách là khách hàng",
                            OrderId = orderId,
                            AccountRoleId = accountRoleCustomer.Id,
                            CreatedById = customerAccount.Id
                        };
                        await _unitOfWork.ReputationLogRepository.AddAsync(customerReputationLog);
                    }

                    if (accountRoleArtisan.TotalReputation < 100)
                    {
                        accountRoleArtisan.TotalReputation += 1;
                        _unitOfWork.AccountRoleRepository.Update(accountRoleArtisan);

                        var artisanReputationLog = new ReputationLog
                        {
                            PointChange = +1,
                            Reason = "Đã hoàn thành đơn hàng với tư cách là nghệ nhân",
                            OrderId = orderId,
                            AccountRoleId = accountRoleArtisan.Id,
                            CreatedById = artisanAccount.Id
                        };
                        await _unitOfWork.ReputationLogRepository.AddAsync(artisanReputationLog);
                    }
                    var saveResult = await _unitOfWork.SaveChangeAsync();
                    if (saveResult <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = "Failed to update order in database."
                        };
                    }
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = $"Error saving changes: {ex.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Error updating order: {ex.Message}"
                };
            }        
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Order updated to Completed successfully."
            };
        }
        public async Task<ResponseModel> Report(Guid orderId, ReportAddModel reportAddModel)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetAsync(orderId, 
                    include: order => order.Include(_ => _.Package).ThenInclude(_ => _.Offer)
                                           .Include(_ => _.Package).ThenInclude(_ => _.Service)
                    );
                if (order == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Order not found."
                    };
                }

                if (order.Stage != OrderStage.AwaitingClosure)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = "Orders can only be reported after they have been delivered."
                    };
                }

                var existingReport = await _unitOfWork.ReportRepository.GetByOrder(orderId);
                if(existingReport != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = "Order was already reported."
                    };
                }

                var report = _mapper.Map<Report>(reportAddModel);
                report.OrderId = orderId;
                report.Status = ReportStatus.Pending;
                report.Code = GenerateCodeHelper.GenerateReportCode();
                await _unitOfWork.ReportRepository.AddAsync(report);

                var newAttachment = new List<ReportAttachment>();
                var attachmentModel = reportAddModel.Attachments;
                if (reportAddModel.Attachments != null)
                {
                    for (int i = 0; i < attachmentModel!.Count; i++)
                    {
                        var attachmentAlt = attachmentModel[i].AttachmentAlt;
                        var attachmentUrl = attachmentModel[i].AttachmentUrl;

                        // string? path = null;
                        
                        // TODO: Fix attachment path
                        // if (attachmentUrl != null)
                        // {
                        //     path = await _cloudinaryHelper.UploadImageAsync(
                        //         attachmentUrl,
                        //         attachmentAlt,
                        //         Guid.NewGuid().ToString(),
                        //         folderName: FolderAttachment.SERVICE
                        //     );
                        // }

                        newAttachment.Add(new ReportAttachment
                        {
                            AttachmentAlt = attachmentAlt,
                            AttachmentUrl = attachmentUrl,
                            ReportId = report.Id
                        });
                    }
                    await _unitOfWork.ReportAttachmentRepository.AddRangeAsync(newAttachment);
                }
                var attachmentModels = _mapper.Map<List<ReportAttachmentModel>>(newAttachment);

                order.Stage = OrderStage.Report;
                _unitOfWork.OrderRepository.Update(order);

                var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Artisan_ReportOrder).Result;
                if (notificationContent != null)
                {
                    var notificationAddModel = new NotificationAddModel
                    {
                        Content = notificationContent.Content.Replace("[#orderCode]", order.Code),
                        AccountId = (Guid)(order.Package.Service != null ? order.Package.Service.CreatedById : order.Package.Offer?.CreatedById)!,
                        NotificationContentId = notificationContent.Id,
                        SourceId = order.Id
                    };
                    await _notificationService.PushNotification(notificationAddModel);
                }

                int result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    var reportModel = _mapper.Map<ReportModel>(report);
                    reportModel.ReportAttachments = attachmentModels;

                    return new ResponseModel
                    {
                        Data = reportModel,
                        Code = StatusCodes.Status201Created,
                        Message = "Report added"
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Failed to report."
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                };
            }
        }
    }
}
