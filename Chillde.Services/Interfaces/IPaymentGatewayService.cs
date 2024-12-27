using Chillde.Services.Models.PaymentModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<string> CreatePaymentUrlVnpay(PaymentInformationAddModel request, HttpContext httpContext);
    }
}
