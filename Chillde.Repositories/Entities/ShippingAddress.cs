namespace Chillde.Repositories.Entities;

 public class ShippingAddress : BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!; 
        public string AddressLine2 { get; set; } = null!;  
        public string WardCode { get; set; } = null!;  
        public string WardName { get; set; } = null!;
        public int DistrictId { get; set; }  
        public int ProvinceId { get; set; }  
        public string DistrictName { get; set; } = null!;
        public string ProvinceName { get; set; } = null!;
        public bool IsDefault { get; set; }

        // Relationship
        public Account CreatedBy { get; set; } = null!;
    }