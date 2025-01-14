using Newtonsoft.Json;

namespace Chillde.Repositories.Models.ShipmentModels
{
    public class ShipmentResponseModel
    {
        [JsonProperty("message")]
        public string? Message { get; set; }
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

}
