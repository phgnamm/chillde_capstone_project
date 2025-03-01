using Newtonsoft.Json;

namespace Chillde.Repositories.Models.ShipmentModels
{
    public class ShipmentResponseModel
    {
        
        [JsonProperty("code")]
        public string? Code { get; set; }
        [JsonProperty("message")]
        public string? Message { get; set; }
        
    }
    public class ShipmentAddResponseModel : ShipmentResponseModel
    {
        [JsonProperty("success")]
        public string? Success { get; set; }
        [JsonProperty("order")]
        public string? Order { get; set; }
    }

    public class ShipmentData
    {
        [JsonProperty("order_code")]
        public string OrderCode { get; set; }
    }
    public class ShipmentCancelResponseModel : ShipmentResponseModel
    {
        [JsonProperty("data")]
        public List<ShipmentData>? Data { get; set; }
    }

    public class OrderInfo
    {
        [JsonProperty("partner_id")]
        public string? PartnerId { get; set; }

        [JsonProperty("label")]
        public string? Label { get; set; }

        [JsonProperty("area")]
        public string? Area { get; set; }

        [JsonProperty("fee")]
        public string? Fee { get; set; }

        [JsonProperty("insurance_fee")]
        public string? InsuranceFee { get; set; }

        [JsonProperty("tracking_id")]
        public long TrackingId { get; set; }

        [JsonProperty("estimated_pick_time")]
        public string? EstimatedPickTime { get; set; }

        [JsonProperty("estimated_deliver_time")]
        public string? EstimatedDeliverTime { get; set; }

        [JsonProperty("products")]
        public List<Product>? Products { get; set; }

        [JsonProperty("status_id")]
        public int StatusId { get; set; }
    }

    public class Product
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("weight")]
        public double Weight { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("product_code")]
        public int ProductCode { get; set; }
    }
}
