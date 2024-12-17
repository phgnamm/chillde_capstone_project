namespace Chillde.Repositories.Entities
{
    public class ShippingAddress : BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string AddressLine { get; set; } = null!;
        public string WardCode { get; set; } = null!;
        public int DistrictId { get; set; }
        public int ProvinceId {  get; set; }
        public bool IsDefault {  get; set; }

        // Foreign key
        public Guid AccountId { get; set; }

        // Relationship
        public Account Account { get; set; } = null!;
    }
}