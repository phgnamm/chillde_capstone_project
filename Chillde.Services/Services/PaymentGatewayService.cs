using Chillde.Services.Interfaces;
using Chillde.Services.Models.PaymentModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        public PaymentGatewayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CreatePaymentUrlVnpay(PaymentInformationAddModel requestDto, HttpContext httpContext)
        {
            var paymentUrl = "";
            var vnPay = new PaymentInformationAddModel
            {
                AccountID = requestDto.AccountID,
                Amount = requestDto.Amount,
                CustomerName = requestDto.CustomerName,
                OrderID = requestDto.OrderID
            };
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var pay = new VNPayLibrary();
            var urlCallBack = $"{_configuration["Vnpay:ReturnUrl"]}/{requestDto.OrderID}";

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)requestDto.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(httpContext));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo",
                $"Khach hang: {requestDto.CustomerName} thanh toan hoa don {requestDto.OrderID}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", requestDto.OrderID);
            paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);


            return paymentUrl;
        }
    }
}
