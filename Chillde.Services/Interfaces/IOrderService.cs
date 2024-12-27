using Chillde.Repositories.Models.OfferModels;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseModel> BalancePayment(OrderAddModel orderModel, HttpContext context);
    }
}
