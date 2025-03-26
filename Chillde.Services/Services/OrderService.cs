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
using Chillde.Repositories.Common;

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

        public OrderService(ISystemConfigService systemConfigService, IUnitOfWork unitOfWork, IClaimService claimService, 
            ICloudinaryHelper cloudinaryHelper, 
            IVnpay vnpay, 
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory         
            )
        {
            _systemConfigService = systemConfigService;
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _vnpay = vnpay;
            _httpClient = httpClientFactory.CreateClient("GhtkClient");
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
            if(orderAddModel.VoucherId != null && orderAddModel.VoucherId is List<Guid> voucherIds)
            {
                await ApplyVoucher(voucherIds, newOrder, PaymentType.Balance);
                foreach(var voucherUsageLog in newOrder.VoucherUsageLogs)
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
            newOrder.Status = OrderStatus.Success;
            _unitOfWork.WalletRepository.Update(wallet);
            await _unitOfWork.OrderRepository.AddAsync(newOrder);
            var result = await _unitOfWork.SaveChangeAsync();
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
                        remainingAmount = (decimal)response.Data;
                    else return response;
                }
                if (!(bool)orderAddModel.WithBalance)
                {
                    newOrder.Transactions.Add(new Transaction
                    {
                        Amount = newOrder.TotalPrice,
                        Type = TransactionType.Deposit,
                        CreatedById = currentUserId.Value,
                        WalletId = wallet.Id,
                        Status = TransactionStatus.Pending
                    });
                    wallet.Balance += (decimal)newOrder.TotalPrice;
                    newOrder.Transactions.Add(new Transaction
                    {
                        Amount = newOrder.TotalPrice,
                        Type = TransactionType.TransferOut,
                        CreatedById = currentUserId.Value,
                        Status = TransactionStatus.Pending,
                        WalletId = wallet.Id,
                    });
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

                var paymentUrl = await GenerateVnPayUrl(newOrder, ipAddress, (decimal)remainingAmount);

                return new ResponseModel { Data = paymentUrl, Message = "Created paymentUrl successfully" };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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

                return await CreateOrderAsync(orderAddModel, package, userId, totalOrder, adminCommission, (int)(package.Offer?.Request?.Quantity ?? 1), null,null);

            }

            var requiredFeatures = package.PackageFeatures
                .Where(_ => _.Feature.IsInformationRequired && (!_.IsExtra ?? true))
                .ToList();
            
            foreach (var feature in requiredFeatures)
            {
                var info = orderAddModel.OrderInformationAddModels?
                    .FirstOrDefault(_ => _.PackageFeatureId == feature.Id);

                if (info == null || string.IsNullOrWhiteSpace(info.Description))
                {
                    throw new InvalidOperationException($"Order description for PackageFeature '{feature.Feature.Name}' cannot be null or empty.");
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
                CreatedById = userId,
                Code = GenerateCodeHelper.GenerateOrderCode(),
                Phone = orderAddModel.Phone,
                Address = orderAddModel.Address,
                ToWard = orderAddModel.ToWard,
                ToDistrict = orderAddModel.ToDistrict,
                ToProvince = orderAddModel.ToProvince,
                TotalPrice = totalOrder + (orderAddModel.ShippingPrice ?? 0),
                DeliveryTime = package.DeliveryTime,
                ShippingPrice = orderAddModel.ShippingPrice,
                OriginPrice = totalOrder,
                AdminCommDefault = adminCommission,
                AdminCommUsedVch = null,
                ArtistRevenue = totalOrder - adminCommission,
                AfterApplyVoucherPrice = null,
                VoucherCost = null,
                Quantity = quantity,
                PackageId = package.Id,
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
                    var adjustedCommission = Math.Max(commissionValue - (commsionVoucherValue ?? 0), 0) / 100;
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
            var checkMaxQuantity = takeExtraFeature.Data.Where(pf =>
                orderAddModel.OrderInformationAddModels!
                    .Any(_ => _.PackageFeatureId == pf.Id && _.Quantity > pf.MaxQuantity));
            if (checkMaxQuantity != null)
            {
                throw new Exception("Quantity in order information cannot greater than max quantity in feature package");
            }
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
                if (voucher == null) throw new Exception( "No valid vouchers found.");


                if (voucher.MinOrderValue.HasValue && remainingOrderPrice < voucher.MinOrderValue.Value)
                {
                    throw new Exception($"The order has at least {voucher.MinOrderValue} to apply this voucher.");
                }
             
                if (voucher.RemainingQuantity.HasValue && voucher.RemainingQuantity.Value <= 0)
                {
                    throw new Exception("This voucher is out of stock to use.");

                }
                if(voucher.ExpiredTime < DateTime.Now)
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
                    CustomerId = (Guid)order.CreatedById,
                    DiscountValue = discount,
                    DiscountValueOrigin = voucher.DiscountValue,
                });
                if (paymentType == PaymentType.Balance)
                {
                    if (voucher.TotalQuantity.HasValue)
                    {
                        voucher.RemainingQuantity -= 1;
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
            order.Transactions.Add(new Transaction
            {
                Amount = remainingAmount,
                Type = TransactionType.Deposit,
                CreatedById = accountId,
                Status = TransactionStatus.Pending,
                WalletId = walletId
            });
            order.Transactions.Add(new Transaction
            {
                Amount = remainingAmount + balance,
                Type = TransactionType.TransferOut,
                CreatedById = accountId,
                Status = TransactionStatus.Pending,
                WalletId = walletId

            });

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
                      .ThenInclude(_ => _.Wallet)
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
                voucherUsageLog.Voucher.RemainingQuantity -= 1;
                voucherUsageLog.UsageStatus = UsageStatus.Used;
            }

            order.Status = OrderStatus.Success;

            var transferOut = order.Transactions.FirstOrDefault(_ => _.Type == TransactionType.TransferOut);
            var deposit = order.Transactions.FirstOrDefault(_ => _.Type == TransactionType.Deposit);

            if (transferOut != null && deposit != null)
            {
                foreach (var transaction in order.Transactions)
                {
                    transaction.Status = TransactionStatus.Completed;
                }

                if (transferOut.Amount > deposit.Amount)
                {
                    var wallet = order.CreatedBy.Wallet;
                    if (wallet == null)
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status404NotFound,
                            Message = "Wallet not found for the user."
                        };

                    wallet.Balance -= (decimal)(transferOut.Amount - deposit.Amount);
                    _unitOfWork.WalletRepository.Update(wallet);
                }
            }

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
            var order = await _unitOfWork.OrderRepository.GetAsync(orderId);
            if (order == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order not found."
                };
            }


            if (order.Stage != OrderStage.Shipping && order.Stage != OrderStage.Return)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status422UnprocessableEntity,
                    Message = "Shipment can only be initiated at the delivery and return stage."
                };
            }

            string partnerId = $"{order.Code}_{order.Stage.GetStringValue()}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            var availableShipment = await _unitOfWork.ShipmentRepository.HasAvalaibleShipment(orderId, partnerId);

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

            var url = "https://services.giaohangtietkiem.vn/services/shipment/order";
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
                    is_freeship = shipmentCreateModel.IsFreeShip,
                    pick_date = shipmentCreateModel.PickDate,
                    deliver_date = shipmentCreateModel.DeliverDate,
                    pick_money = shipmentCreateModel.PickMoney,
                    note = shipmentCreateModel.Note,
                    value = shipmentCreateModel.Value,
                    transport = shipmentCreateModel.Transport,
                    pick_option = shipmentCreateModel.PickOption,
                    deliver_option = shipmentCreateModel.DeliverOption,
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
                    StatusId = (ShipmentStatus)(parsedJson.Order?.StatusId ?? 0),
                    PartnerId = parsedJson!.Order!.PartnerId,
                    Label = parsedJson.Order.Label,
                    Area = parsedJson.Order.Area,
                    Fee = parsedJson.Order.Fee != null ? decimal.Parse(parsedJson.Order.Fee) : 0,
                    InsuranceFee = parsedJson.Order.InsuranceFee != null ? decimal.Parse(parsedJson.Order.InsuranceFee) : 0,
                    EstimatedPickTime = parsedJson.Order.EstimatedPickTime,
                    EstimatedDeliverTime = parsedJson.Order.EstimatedDeliverTime,
                };

                await _unitOfWork.ShipmentRepository.AddAsync(shipment);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Success",
                    Data = shipment
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
            var orders = await _unitOfWork.OrderRepository.GetAllAsync(
                filter: _ => (_.Package.CreatedById == currentUserId.Value) || (_.CreatedById == currentUserId.Value) || (orderFilterModel.Status.HasValue && _.Status == orderFilterModel.Status),
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
                TotalPrice = _.TotalPrice,
                PackagePrice = _.OriginPrice,
                PackageName = _.Package.Name.ToString(),
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
            var order = await _unitOfWork.OrderRepository.GetAsync(orderId);
            if (order == null)
            {
                return new ResponseModel
                {
                    Message = "Not found",
                    Code = StatusCodes.Status404NotFound
                };
            }
                order.Status = (OrderStatus)orderStatus;
                if (orderStatus == OrderStatus.Accepted)
                {
                    order.Stage = OrderStage.SketchInProcess;
                }
            
            _unitOfWork.OrderRepository.Update(order);
            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0
                ? new ResponseModel { Message = "Successfully" }
                : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Fail" };
        }

        public async Task<ResponseModel> UsedAdminVoucher(Guid orderId, Guid voucherId)
        {
            var order = await _unitOfWork.OrderRepository.GetAsync(orderId);
            if (order == null)
            {
                return new ResponseModel
                {
                    Message = "Order not found.",
                    Code = StatusCodes.Status404NotFound
                };
            }

            if (order.Status != OrderStatus.Success)
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

            if (voucher.TotalQuantity.HasValue && voucher.TotalQuantity.Value < 1)
            {
                return new ResponseModel
                {
                    Message = "Voucher is out of stock.",
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
                CustomerId = (Guid)order.CreatedById,
                DiscountValue = (decimal)(order.AdminCommDefault - adminCommAfterUsed),
                DiscountValueOrigin = voucherDiscountValue,
                UsageStatus = UsageStatus.Used
            });
            if (voucher.TotalQuantity.HasValue)
            {
                voucher.RemainingQuantity -= 1;
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
    }
}
