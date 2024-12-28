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

        public Task<ResponseModel> BalancePayment(OrderAddModel orderModel, HttpContext context)
        {
            throw new NotImplementedException();
        }

        //public async Task<ResponseModel> CreatePaymentUrl(OrderAddModel order, string ipAddress)
        //{
        //    var currentUserId = _claimService.GetCurrentUserId;
        //    if (!currentUserId.HasValue)
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status401Unauthorized,
        //            Message = "Unauthorized"
        //        };

        //    var package = await _unitOfWork.PackageRepository.Get(order.PackageId);
        //    if (package == null)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status404NotFound,
        //            Message = "Package not found."
        //        };
        //    }

        //    decimal totalPrice = (decimal)package.Price;

        //    var newOrder = new Order
        //    {
        //        CreatedById = currentUserId.Value,
        //        Phone = order.Phone,
        //        Address = order.Address,
        //        TotalPrice = totalPrice,
        //        PackagePrice = package.Price,
        //        Quantity = order.Quantity,
        //        PackageId = order.PackageId
        //    };
        //    if(order.OrderInformationAddModels != null)
        //    {
        //        newOrder.OrderInformations = order.OrderInformationAddModels.Select(_ => new OrderInformation
        //        {
        //            Description = _.Description,
        //            PackageFeatureId = _.PackageFeatureId
        //        }).ToList();
        //        var extraFeatureCost = await _unitOfWork.PackageFeatureRepository.SumPriceOfExtraFeatures(order.OrderInformationAddModels.Select(_ => _.PackageFeatureId).ToList());

        //        if (extraFeatureCost > 0)
        //        {
        //            totalPrice += extraFeatureCost;
        //            newOrder.TotalPrice = totalPrice;
        //        }
        //    }
        //    if (order.WithBalance == true)
        //    {
        //        var wallet = await _unitOfWork.WalletRepository.GetWalletByAccount(currentUserId.Value);
        //        var balance = wallet.Balance;
        //        if (balance <= 0) {
        //            return new ResponseModel
        //            {
        //                Message = "Your balance do not have enough money to order",
        //                Code = StatusCodes.Status400BadRequest
        //            };
        //        }
        //        if(balance > totalPrice)
        //        {
        //            return new ResponseModel
        //            {
        //                Message = "Your balance greater than total price in the order, if you want to checkout with balance, please select the checkout with balance!",
        //                Code = StatusCodes.Status400BadRequest
        //            };
        //        }
        //        var checkVnpayValid = totalPrice - balance;
        //        if (checkVnpayValid < 5000)
        //        {
        //            return new ResponseModel
        //            {
        //                Message = $"Cannot checkout vnPay with '{checkVnpayValid}VND'!",
        //                Code = StatusCodes.Status400BadRequest
        //            };
        //        }
        //        wallet.Balance -= balance;

        //        var walletHistory = new WalletHistory
        //        {
        //            WalletId = wallet.Id,
        //            Amount = balance,
        //            Type = WalletHistoryType.TransferOut, 
        //            Status = WalletHistoryStatus.Completed, 
        //        };
        //        wallet.WalletHistories.Add(walletHistory);

        //        // Lưu thay đổi vào cơ sở dữ liệu
        //        _unitOfWork.WalletRepository.Update(wallet);

        //        newOrder.Payments = order.PaymentAddModels.Select(payment => new Payment
        //        {
        //            PaymentType = PaymentType.VnPay,
        //            Amount = payment.Amount,
        //        }).ToList();

        //    }
        //    await _unitOfWork.OrderRepository.AddAsync(newOrder);
        //    var check = await _unitOfWork.SaveChangeAsync();
        //    if(check < 0)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status400BadRequest,
        //            Message = "Fail to save"
        //        };
        //    }

        //    var paymentRequest = new PaymentRequest
        //    {
        //        PaymentId = newOrder.Id,
        //        Money = (double)newOrder.TotalPrice,
        //        Description = $"Payment for order {newOrder.Id}",
        //        IpAddress = ipAddress,
        //        BankCode = BankCode.ANY,
        //        CreatedDate = DateTime.Now,
        //        Currency = Currency.VND,
        //        Language = DisplayLanguage.Vietnamese
        //    };
        //    var result = await _vnpay.GetPaymentUrl(paymentRequest);
        //    return new ResponseModel
        //    {
        //        Data = result
        //    };
        //}
        public async Task<ResponseModel> CreatePaymentUrl(OrderAddModel order, string ipAddress)
        {
            // Lấy thông tin người dùng hiện tại
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
                return UnauthorizedResponse();

            // Kiểm tra package
            var package = await _unitOfWork.PackageRepository.Get(order.PackageId);
            if (package == null)
                return NotFoundResponse("Package not found.");

            // Tính tổng giá
            decimal totalPrice = (decimal)package.Price;
            var newOrder = InitializeOrder(order, package, currentUserId.Value, ref totalPrice);

            // Xử lý thông tin bổ sung nếu có
            if (order.OrderInformationAddModels != null)
                await ProcessExtraFeatures(order, newOrder,  totalPrice);

            // Xử lý thanh toán với số dư (Balance)
            if (order.WithBalance == true)
            {
                var wallet = await _unitOfWork.WalletRepository.GetWalletByAccount(currentUserId.Value);
                var response = ProcessWalletPayment(wallet, ref totalPrice, newOrder);
                if (response != null) return response;
            }

            // Lưu đơn hàng vào cơ sở dữ liệu
            await _unitOfWork.OrderRepository.AddAsync(newOrder);
            if (await _unitOfWork.SaveChangeAsync() < 0)
                return BadRequestResponse("Failed to save order.");

            // Tạo URL thanh toán VnPay
            var paymentUrl = await GenerateVnPayUrl(newOrder, ipAddress);

            return new ResponseModel { Data = paymentUrl };
        }
        private Order InitializeOrder(OrderAddModel order, Package package, Guid userId, ref decimal totalPrice)
        {
            return new Order
            {
                CreatedById = userId,
                Phone = order.Phone,
                Address = order.Address,
                TotalPrice = totalPrice,
                PackagePrice = package.Price,
                Quantity = order.Quantity,
                PackageId = order.PackageId,
                OrderInformations = order.OrderInformationAddModels?.Select(info => new OrderInformation
                {
                    Description = info.Description,
                    PackageFeatureId = info.PackageFeatureId
                }).ToList()
            };
        }
        private async Task ProcessExtraFeatures(OrderAddModel order, Order newOrder, decimal totalPrice)
        {
            var featureIds = order.OrderInformationAddModels.Select(info => info.PackageFeatureId).ToList();
            var extraFeatureCost = await _unitOfWork.PackageFeatureRepository.SumPriceOfExtraFeatures(featureIds);

            if (extraFeatureCost > 0)
            {
                totalPrice += extraFeatureCost;
                newOrder.TotalPrice = totalPrice;
            }
        }
        private ResponseModel? ProcessWalletPayment(Wallet wallet, ref decimal totalPrice, Order order)
        {
            var balance = wallet.Balance;

            if (balance <= 0)
                return BadRequestResponse("Your balance does not have enough money to order.");

            if (balance > totalPrice)
                return BadRequestResponse("Your balance is greater than the total price in the order.");

            var remainingAmount = totalPrice - balance;
            if (remainingAmount < 5000)
                return BadRequestResponse($"Cannot checkout VNPay with '{remainingAmount} VND'.");

            // Trừ số dư trong ví và tạo lịch sử giao dịch
            wallet.Balance -= balance;
            wallet.WalletHistories.Add(new WalletHistory
            {
                WalletId = wallet.Id,
                Amount = balance,
                Type = WalletHistoryType.TransferOut,
                Status = WalletHistoryStatus.Completed
            });

            // Thêm phương thức thanh toán với số dư
            order.Payments.Add(new Payment
            {
                PaymentType = PaymentType.Balance,
                Amount = balance
            });

            // Thêm phương thức thanh toán với VNPay
            order.Payments.Add(new Payment
            {
                PaymentType = PaymentType.VnPay,
                Amount = remainingAmount
            });

            _unitOfWork.WalletRepository.Update(wallet);
            return null;
        }
        private async Task<string> GenerateVnPayUrl(Order order, string ipAddress)
        {
            var paymentRequest = new PaymentRequest
            {
                PaymentId = order.Id,
                Money = (double)order.TotalPrice,
                Description = $"Payment for order {order.Id}",
                IpAddress = ipAddress,
                BankCode = BankCode.ANY,
                CreatedDate = DateTime.Now,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese
            };

            return await _vnpay.GetPaymentUrl(paymentRequest);
        }
        private ResponseModel UnauthorizedResponse()
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized"
            };
        }

        private ResponseModel NotFoundResponse(string message)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = message
            };
        }

        private ResponseModel BadRequestResponse(string message)
        {
            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = message
            };
        }



    }
}
