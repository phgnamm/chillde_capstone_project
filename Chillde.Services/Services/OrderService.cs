using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Repositories.Models.VnPayModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _vnpay = vnpay;
            _httpClient = httpClientFactory.CreateClient("GhnClient");
            _shopId = configuration["GhnSettings:ShopId"];
            _token = configuration["GhnSettings:Token"];
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

            var walletHistory = new WalletHistory
            {
                WalletId = wallet.Id,
                Amount = totalPrice,
                Type = WalletHistoryType.TransferOut,
                Status = WalletHistoryStatus.Completed
            };
            wallet.WalletHistories.Add(walletHistory);

            _unitOfWork.WalletRepository.Update(wallet);
            newOrder.Payments.Add(new Payment
            {
                PaymentType = PaymentType.Balance,
                Amount = totalPrice,
                PaymentStatus = PaymentStatus.Success
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
                var response = await ProcessWalletPayment(wallet, totalPrice, newOrder);
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
            var requiredFeatures = package.PackageFeatures.Where(_ => _.IsInformationRequired == true).ToList();

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
                Phone = orderAddModel.Phone,
                Address = orderAddModel.Address,
                TotalPrice = totalPrice * orderAddModel.Quantity,
                PackagePrice = package.Price,
                Quantity = orderAddModel.Quantity,
                PackageId = orderAddModel.PackageId,
                OrderInformations = orderAddModel?.OrderInformationAddModels?.Select(_ => new OrderInformation
                {
                    Description = _.Description,
                    PackageFeatureId = _.PackageFeatureId
                }).ToList()
            };
        }
        private async Task ProcessExtraFeatures(OrderAddModel orderAddModel, Repositories.Entities.Order newOrder, decimal totalPrice)
        {
            var featureIds = orderAddModel.OrderInformationAddModels.Select(_ => _.PackageFeatureId).ToList();
            var extraFeatureCost = await _unitOfWork.PackageFeatureRepository.SumPriceOfExtraFeatures(featureIds);

            if (extraFeatureCost > 0)
            {
                totalPrice += extraFeatureCost;
                newOrder.TotalPrice = totalPrice;
            }
        }
        private async Task<ResponseModel> ProcessWalletPayment(Wallet wallet, decimal totalPrice, Repositories.Entities.Order order)
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
                PaymentStatus = PaymentStatus.Pending
            });

            order.Payments.Add(new Payment
            {
                PaymentType = PaymentType.VnPay,
                Amount = (decimal)remainingAmount,
                PaymentStatus = PaymentStatus.Pending
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

            if (order.Status == OrderStatus.Success)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Order is already completed."
                };
            }

            order.Status = OrderStatus.Success;
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

                var walletHistory = new WalletHistory
                {
                    WalletId = wallet.Id,
                    Amount = balancePayment.Amount,
                    Type = WalletHistoryType.TransferOut,
                    Status = WalletHistoryStatus.Completed
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
        public async Task<ResponseModel> CreateShipmentAsync(ShipmentAddModel model, Guid orderId)
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

                if (order.ShipmentCode != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status406NotAcceptable,
                        Message = "Đơn hàng đã có đơn vận chuyển"
                    };
                }

                var customer = await _unitOfWork.AccountRepository.GetAsync((Guid)order.CreatedById);
                if (customer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Customer not found."
                    };
                }

                _httpClient.DefaultRequestHeaders.Clear();
                //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                //_httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);
                //_httpClient.DefaultRequestHeaders.Add("Token", _token);

                var payload = new
                {
                    payment_type_id = 2,
                    note = model.Note,
                    required_note = model.RequiredNote.ToString(),
                    from_name = model.FromName,
                    from_phone = model.FromPhone,
                    from_address = model.FromAddress,
                    from_ward_name = model.FromWard,
                    from_district_name = model.FromDistrict,
                    from_province_name = model.FromProvince,
                    //return_phone = (string?)null,
                    //return_address = (string?)null,
                    //return_district_id = (string?)null,
                    //return_ward_code = "",
                    //client_order_code = "",
                    to_name = customer.FirstName + " " + customer.LastName,
                    to_phone = order.Phone,
                    to_address = order.Address,
                    to_ward_code = order.ToWard,
                    to_district_id = order.ToDistrict,
                    //cod_amount = (int?)null,
                    //content = (string?)null,
                    weight = model.Weight,
                    length = model.Length,
                    width = model.Width,
                    height = model.Height,
                    //pick_station_id = (int?)null,
                    //deliver_station_id = (int?)null,
                    insurance_value = 0,
                    //service_id = (int?)null,
                    service_type_id = 2,
                    //coupon = (string?)null,
                    //pick_shift = (int[]?)null,
                    items = new[]
                {
                        new
                        {
                            name = model.ItemName,
                            //code = (string?)null,
                            quantity = model.ItemQuantity,
                            price = model.ItemPrice,
                            //length = (int?)null,
                            //width = (int?)null,
                            //height = (int?)null,
                            weight = model.ItemWeight,
                            //category = new { level1 = "Áo" }
                        }
                    }
                };

                //var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                //var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                //var response = await _httpClient.PostAsync(_url, content);

                //var responseContent = await response.Content.ReadAsStringAsync();
                //var result = JsonConvert.DeserializeObject<ShipmentResponseModel>(responseContent);

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);
                _httpClient.DefaultRequestHeaders.Add("Token", _token);
                var response = await _httpClient.PostAsync("v2/shipping-order/create", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ShipmentAddResponseModel>(responseContent);

                if (result.Code == StatusCodes.Status200OK.ToString())
                {
                    order.ShipmentCode = result.Data.OrderCode;
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
    }
}
