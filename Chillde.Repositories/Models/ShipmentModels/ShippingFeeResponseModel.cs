namespace Chillde.Repositories.Models.ShipmentModels;

public class ShippingFeeResponseModel
{
    public int Total { get; set; }
    public int ServiceFee { get; set; }
    public int InsuranceFee { get; set; }
    public int PickStationFee { get; set; }
    public int ReturnStationFee { get; set; }
}