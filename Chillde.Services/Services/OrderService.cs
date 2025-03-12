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

        public OrderService(IUnitOfWork unitOfWork, IClaimService claimService, 
            ICloudinaryHelper cloudinaryHelper, 
            IVnpay vnpay, 
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory
            )
        {
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

            var package = await _unitOfWork.PackageRepository.Get(orderAddModel.PackageId);
            if (package == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Package not found"
                };

            decimal totalPrice = (decimal)package.Price;
            var newOrder = await InitializeOrder(orderAddModel, package, currentUserId.Value, totalPrice);

            if (orderAddModel.OrderInformationAddModels != null)
                await ProcessExtraFeatures(orderAddModel, newOrder, totalPrice);

            var wallet = await _unitOfWork.WalletRepository.GetWalletByAccount(currentUserId.Value);
            if (wallet == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Wallet not found"
                };

            if (wallet.Balance < totalPrice)
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Your balance does not have enough money to complete this order"
                };

            wallet.Balance -= totalPrice;

            var walletHistory = new Transaction()
            {
                WalletId = wallet.Id,
                Amount = totalPrice,
                Type = TransactionType.TransferOut,
                Status = TransactionStatus.Completed,
                CreatedById = currentUserId.Value
            };
            wallet.WalletHistories.Add(walletHistory);

            _unitOfWork.WalletRepository.Update(wallet);
            newOrder.Payments.Add(new Payment
            {
                PaymentType = PaymentType.Balance,
                Amount = totalPrice,
                PaymentStatus = PaymentStatus.Success,
                CreatedById = currentUserId.Value

            });
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
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            var package = await _unitOfWork.PackageRepository.Get(orderAddModel.PackageId);
            if (package == null)
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Package not found."
                };

            decimal totalPrice = (decimal)package.Price;
            decimal remainingAmount = 0;

            var newOrder = await InitializeOrder(orderAddModel, package, currentUserId.Value, totalPrice);

            if (orderAddModel.OrderInformationAddModels != null)
                await ProcessExtraFeatures(orderAddModel, newOrder, totalPrice);

            if ((bool)orderAddModel.WithBalance)
            {
                var wallet = await _unitOfWork.WalletRepository.GetWalletByAccount(currentUserId.Value);
                var response = await ProcessWalletPayment(wallet, totalPrice, newOrder, currentUserId.Value);
                if (response != null) return response;
                remainingAmount = (decimal)response.Data;
            }
            if (!(bool)orderAddModel.WithBalance)
            {
                newOrder.Payments.Add(new Payment
                {
                    PaymentType = PaymentType.VnPay,
                    Amount = totalPrice,
                    PaymentStatus = PaymentStatus.Pending,
                    CreatedById = currentUserId.Value

                });
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
        private async Task<Repositories.Entities.Order> InitializeOrder(OrderAddModel orderAddModel, Package package, Guid userId, decimal totalPrice)
        {
            var requiredFeatures = package.PackageFeatures.Where(_ => _.IsExtra == true).ToList();

            foreach (var feature in requiredFeatures)
            {
                var correspondingInfo = orderAddModel.OrderInformationAddModels?
                    .FirstOrDefault(_ => _.PackageFeatureId == feature.Id);

                if (correspondingInfo == null || string.IsNullOrWhiteSpace(correspondingInfo.Description))
                {
                    new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = $"Order description for PackageFeature '{feature.Feature.Name}' cannot be null or empty when a question is present."
                    };
                }
            }
            return new Repositories.Entities.Order
            {
                CreatedById = userId,
                Code = GenerateCodeHelper.GenerateOrderCode(),
                Phone = orderAddModel.Phone,
                Address = orderAddModel.Address,
                TotalPrice = totalPrice * orderAddModel.Quantity,
                PackagePrice = package.Price,
                Quantity = orderAddModel.Quantity,
                PackageId = orderAddModel.PackageId,
                OrderInformations = orderAddModel.OrderInformationAddModels!.Select(_ => new OrderInformation
                {
                    Quantity = _.Quantity ?? null,
                    Price = _.Price ?? null,
                    Description = _.Description ?? null,
                    PackageFeatureId = _.PackageFeatureId
                }).ToList()
            };
        }
        private async Task ProcessExtraFeatures(OrderAddModel orderAddModel, Repositories.Entities.Order newOrder, decimal totalPrice)
        {
            var extraFeatureCost = orderAddModel.OrderInformationAddModels!.Sum(_ => _.Quantity * _.Price);
            //var extraFeatureCost = await _unitOfWork.PackageFeatureRepository.SumPriceOfExtraFeatures(featureIds);

            if (extraFeatureCost > 0)
            {
                totalPrice += (decimal)extraFeatureCost;
                newOrder.TotalPrice = totalPrice;
            }
        }
        private async Task<ResponseModel> ProcessWalletPayment(Wallet wallet, decimal totalPrice, Repositories.Entities.Order order, Guid accountId)
        {
            var balance = wallet.Balance;

            if (balance <= 0)
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Your balance does not have enough money to order"
                };

            if (balance > totalPrice)
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

            order.Payments.Add(new Payment
            {
                PaymentType = PaymentType.Balance,
                Amount = balance,
                PaymentStatus = PaymentStatus.Pending,
                CreatedById = accountId

            });

            order.Payments.Add(new Payment
            {
                PaymentType = PaymentType.VnPay,
                Amount = (decimal)remainingAmount,
                PaymentStatus = PaymentStatus.Pending,
                CreatedById = accountId


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
                _ => _.Include(_ => _.CreatedBy).Include(_ => _.Payments)
            );

            if (order == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order not found."
                };
            }

            if (order.Status == OrderStatus.Accepted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Order is already completed."
                };
            }

            order.Status = OrderStatus.Accepted;
            foreach (var payment in order.Payments)
            {
                payment.PaymentStatus = PaymentStatus.Success;
            }

            var balancePayment = order.Payments.FirstOrDefault(_ => _.PaymentType == PaymentType.Balance);
            if (balancePayment != null)
            {
                var wallet = await _unitOfWork.WalletRepository.GetWalletByAccount((Guid)order.CreatedById);
                if (wallet == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Wallet not found for the user."
                    };
                }

                var walletHistory = new Transaction()
                {
                    WalletId = wallet.Id,
                    Amount = balancePayment.Amount,
                    Type = TransactionType.TransferOut,
                    Status = TransactionStatus.Completed,
                    CreatedById = order.CreatedById,
                };

                wallet.WalletHistories.Add(walletHistory);
                wallet.Balance -= balancePayment.Amount;

                _unitOfWork.WalletRepository.Update(wallet);
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

            string partnerId = $"{order.Code}_{order.Stage.GetStringValue()}";

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
                PackagePrice = _.PackagePrice,
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

        public async Task<ResponseModel> UpdateStatus(Guid orderId, OrderStatus orderStatus)
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
            order.Status = orderStatus;
            _unitOfWork.OrderRepository.Update(order);
            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0
                ? new ResponseModel { Message = "Successfully" }
                : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Fail" };
        }
    }
}
