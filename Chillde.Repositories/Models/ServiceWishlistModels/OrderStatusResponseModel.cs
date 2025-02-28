using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.ServiceWishlistModels
{
    public class OrderStatusResponseModel
    {
        public string? LabelId { get; set; }
        public string? PartnerId { get; set; }
        public string? Status { get; set; }
        public string? StatusText { get; set; }
        public string? Created { get; set; }
        public string? Modified { get; set; }
        public string? Message { get; set; }
        public string? PickDate { get; set; }
        public string? DeliverDate { get; set; }
        public string? CustomerFullname { get; set; }
        public string? CustomerTel { get; set; }
        public string? Address { get; set; }
        public int? StorageDay { get; set; }
        public int? ShipMoney { get; set; }
        public int? Insurance { get; set; }
        public int? Value { get; set; }
        public int? Weight { get; set; }
        public int? PickMoney { get; set; }
        public int? IsFreeShip { get; set; }
    }
}
