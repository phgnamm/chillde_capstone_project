using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.VnPayModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
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


        public OrderService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IVnpay vnpay, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _vnpay = vnpay;
            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);

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
            var newOrder = InitializeOrder(orderAddModel, package, currentUserId.Value, totalPrice);

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
                    Code = StatusCodes.Status401Unauthorized,
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
            var newOrder = InitializeOrder(orderAddModel, package, currentUserId.Value, totalPrice);

            if (orderAddModel.OrderInformationAddModels != null)
                await ProcessExtraFeatures(orderAddModel, newOrder,  totalPrice);

            if ((bool)orderAddModel.WithBalance)
            {
                var wallet = await _unitOfWork.WalletRepository.GetWalletByAccount(currentUserId.Value);
                var response = ProcessWalletPayment(wallet, totalPrice, newOrder);
                if (response != null) return response;
            }
            if(!(bool)orderAddModel.WithBalance)
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

            var paymentUrl = await GenerateVnPayUrl(newOrder, ipAddress);

            return new ResponseModel { Data = paymentUrl, Message = "Created paymentUrl successfully" };
        }
        private Repositories.Entities.Order InitializeOrder(OrderAddModel orderAddModel, Package package, Guid userId, decimal totalPrice)
        {
            return new Repositories.Entities.Order
            {
                CreatedById = userId,
                Phone = orderAddModel.Phone,
                Address = orderAddModel.Address,
                TotalPrice = totalPrice,
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
        private ResponseModel? ProcessWalletPayment(Wallet wallet, decimal totalPrice, Repositories.Entities.Order orderAddModel)
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

            var remainingAmount = totalPrice - balance;
            if (remainingAmount < 5000)
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = $"Cannot checkout VNPay with '{remainingAmount} VND'. Please checkout with just vnPay payment!"
                };

            wallet.Balance -= balance;
            wallet.WalletHistories.Add(new WalletHistory
            {
                WalletId = wallet.Id,
                Amount = balance,
                Type = WalletHistoryType.TransferOut,
                Status = WalletHistoryStatus.InProcess
            });

            orderAddModel.Payments.Add(new Payment
            {
                PaymentType = PaymentType.Balance,
                Amount = balance,
                PaymentStatus = PaymentStatus.Pending
            });

            orderAddModel.Payments.Add(new Payment
            {
                PaymentType = PaymentType.VnPay,
                Amount = remainingAmount,
                PaymentStatus = PaymentStatus.Pending
            });

            _unitOfWork.WalletRepository.Update(wallet);
            return null;
        }
        private async Task<string> GenerateVnPayUrl(Repositories.Entities.Order orderAddModel, string ipAddress)
        {
            var paymentRequest = new PaymentRequest
            {
                PaymentId = orderAddModel.Id,
                Money = (double)orderAddModel.TotalPrice,
                Description = $"Payment for order {orderAddModel.Id}",
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

            var balancePayment = order.Payments.FirstOrDefault(p => p.PaymentType == PaymentType.Balance);
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


    }
}
