namespace Chillde.Services.Models.ShipmentModels;

public class ShippingFeeRequestModel
{
    public int FromDistrictId { get; set; }
    
    public string? FromWardCode { get; set; }
    public int ToDistrictId { get; set; }
    public string? ToWardCode { get; set; }
    public int ServiceId { get; set; }
    public int? ServiceTypeId { get; set; }
    public int Weight { get; set; } 
    public int? Length { get; set; } 
    public int? Width { get; set; } 
    public int? Height { get; set; } 
    public decimal? InsuranceValue { get; set; } 
    public decimal? CodFailedAmount { get; set; } 
    public string? Coupon { get; set; } 
}