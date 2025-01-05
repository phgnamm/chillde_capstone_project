using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ShippingAddressModels
{
    public class ShippingAddressAddModel
    {
        public string FullName { get; set; } = null!;
        [Phone] [StringLength(15)]
        public string PhoneNumber { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string AddressLine2 { get; set; } = null!;
        public string WardCode { get; set; } = null!;
        public int DistrictId { get; set; }
        public int ProvinceId { get; set; }
    }
}
