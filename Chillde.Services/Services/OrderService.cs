using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class OrderService : IOrderService
    {
        public Task<ResponseModel> BalancePayment(OrderAddModel orderModel, HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
