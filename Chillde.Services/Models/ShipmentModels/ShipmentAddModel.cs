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
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        [Required]
        public int InsuranceValue { get; set; } = 0; //Use to declare parcel value. GHN will base on this value for compensation if any unexpected things happen (lost, broken...).
                                                     //Maximum 5.000.000
                                                     //Default value: 0
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
        public string? Note { get; set; } //Client note for shipper.
        //public Guid OrderId { get; set; }
    }
}
