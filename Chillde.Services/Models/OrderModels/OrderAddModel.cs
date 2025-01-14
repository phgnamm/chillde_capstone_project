using Chillde.Repositories.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.OrderModels
{
    public class OrderAddModel
    {
        [Required]
        public required string Phone { get; set; }
        [Required]
        public required string Address { get; set; }
        [Required]
        public required decimal TotalPrice { get; set; }
        [Required]
        public required int Quantity { get; set; }
        [Required]
        public Guid PaymentId { get; set; }
        [Required]
        public Guid PackageId { get; set; }
        [Required]
        public Guid ShipmentId { get; set; }
    }
}
