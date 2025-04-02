using Newtonsoft.Json;

public class ShippingFeeResponseModel
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("fee")]
    public int Fee { get; set; }

    [JsonProperty("insurance_fee")]
    public int InsuranceFee { get; set; }

    [JsonProperty("include_vat")]
    public int IncludeVat { get; set; }

    [JsonProperty("cost_id")]
    public int CostId { get; set; }

    [JsonProperty("delivery_type")]
    public string DeliveryType { get; set; }

    [JsonProperty("a")]
    public int A { get; set; }

    [JsonProperty("dt")]
    public string Dt { get; set; }

    [JsonProperty("extFees")]
    public List<object> ExtFees { get; set; } // Mảng rỗng trong JSON, có thể là danh sách các phí bổ sung

    [JsonProperty("promotion_key")]
    public string PromotionKey { get; set; }

    [JsonProperty("delivery")]
    public bool Delivery { get; set; }

    [JsonProperty("ship_fee_only")]
    public int ShipFeeOnly { get; set; }

    [JsonProperty("distance")]
    public int Distance { get; set; }

    [JsonProperty("options")]
    public ShippingFeeOptions Options { get; set; }
}

public class ShippingFeeOptions
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("shipMoney")]
    public int ShipMoney { get; set; }

    [JsonProperty("shipMoneyText")]
    public string ShipMoneyText { get; set; }

    [JsonProperty("vatText")]
    public string VatText { get; set; }

    [JsonProperty("desc")]
    public string Desc { get; set; }

    [JsonProperty("coupon")]
    public string Coupon { get; set; }

    [JsonProperty("maxUses")]
    public int MaxUses { get; set; }

    [JsonProperty("maxDates")]
    public int MaxDates { get; set; }

    [JsonProperty("maxDateString")]
    public string MaxDateString { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("activatedDate")]
    public string ActivatedDate { get; set; }

    [JsonProperty("couponTitle")]
    public string CouponTitle { get; set; }

    [JsonProperty("discount")]
    public string Discount { get; set; }

    [JsonProperty("couponId")]
    public int CouponId { get; set; }
}