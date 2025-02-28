namespace Chillde.Repositories.Models.ShipmentModels;

public class ShippingFeeResponseModel
{
    public string? Name { get; set; }
    public int Fee { get; set; }
    public int InsuranceFee { get; set; }
    public int IncludeVat { get; set; }
    public int CostId { get; set; }
    public string? DeliveryType { get; set; }
    public int A { get; set; }
    public string? Dt { get; set; }
    public List<ExtFee>? ExtFees { get; set; }
    public string? PromotionKey { get; set; }
    public bool Delivery { get; set; }
    public int ShipFeeOnly { get; set; }
    public double Distance { get; set; }
    public Options? Options { get; set; }
}

public class ExtFee
{
    public string? Display { get; set; }
    public string? Title { get; set; }
    public int Amount { get; set; }
    public string? Type { get; set; }
}

public class Options
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public int ShipMoney { get; set; }
    public string? ShipMoneyText { get; set; }
    public string? VatText { get; set; }
    public string? Desc { get; set; }
    public string? Coupon { get; set; }
    public int MaxUses { get; set; }
    public int MaxDates { get; set; }
    public string? MaxDateString { get; set; }
    public string? Content { get; set; }
    public string? ActivatedDate { get; set; }
    public string? CouponTitle { get; set; }
    public string? Discount { get; set; }
    public int CouponId { get; set; }
}