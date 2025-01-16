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
        [JsonProperty("code_message_value")]
        public string? MessageValue { get; set; }
        [JsonProperty("message_display")]
        public string? MessageDisplay { get; set; }
        [JsonProperty("data")]
        public ShipmentData? Data { get; set; }
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
}
