using Chillde.Repositories.Enums;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ShipmentModels
{
    public class ShipmentAddModel
    {
        [Required]
        public required string FromName {  get; set; }
        [Required]
        public required string FromPhone { get; set; }
        [Required]
        public required string FromAddress { get; set; }
        [Required]
        public required string FromWard { get; set; }
        [Required]
        public required string FromDistrict { get; set; }
        [Required]
        public required string FromProvince { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public int Length { get; set; }
        [Required]
        public int Width { get; set; }
        [Required]
        public int Height { get; set; }
        [Required]
        public ShipmentUser PaymentTypeId { get; set; } //Choose who pay shipping fee (0.Artisan, 1.Customer)
        [Required]
        public ShipmentNote RequiredNote { get; set; } //Note shipping order.Allowed values: 0.CHOTHUHANG, 1.CHOXEMHANGKHONGTHU, 2.KHONGCHOXEMHANG 
        [Required]
        public string ItemName { get; set; }
        [Required]
        public int ItemQuantity { get; set; }
        [Required]
        public int ItemPrice { get; set; }
        [Required]
        public int ItemWeight { get; set; }
        public string? Note { get; set; }
        //public Guid OrderId { get; set; }
    }
}
