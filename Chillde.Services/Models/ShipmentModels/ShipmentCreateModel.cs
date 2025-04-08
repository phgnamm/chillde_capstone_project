using Newtonsoft.Json;

namespace Chillde.Services.Models.ShipmentModels
{
    public class ShipmentCreateModel
    {
        #region Artisan's information
        //[JsonProperty("order.id")]
        //public string? Id { get; set; } // Thêm trường Id từ JSON

        [JsonProperty("order.pick_name")]
        public required string PickName { get; set; }

        [JsonProperty("order.pick_address")]
        public required string PickAddress { get; set; }

        [JsonProperty("order.pick_province")]
        public required string PickProvince { get; set; }

        [JsonProperty("order.pick_district")]
        public required string PickDistrict { get; set; }

        [JsonProperty("order.pick_ward")]
        public required string PickWard { get; set; }

        [JsonProperty("order.pick_tel")]
        public required string PickTel { get; set; }
        #endregion

        #region Customer's information
        [JsonProperty("order.name")]
        public required string Name { get; set; }

        [JsonProperty("order.address")]
        public required string Address { get; set; }

        [JsonProperty("order.province")]
        public required string Province { get; set; }

        [JsonProperty("order.district")]
        public required string District { get; set; }

        [JsonProperty("order.ward")]
        public required string Ward { get; set; }

        [JsonProperty("order.tel")]
        public required string Tel { get; set; }

        [JsonProperty("order.hamlet")]
        public required string Hamlet { get; set; }

        [JsonProperty("order.email")]
        public string? Email { get; set; } // Không bắt buộc vì JSON không có
        #endregion

        /*[JsonProperty("order.is_freeship")]
        public int IsFreeShip { get; set; }*/
        //[JsonProperty("order.deliver_option")]
        //public string? Fee { get; set; }
        //[JsonProperty("order.value")]
        //public string? InsuranceFee { get; set; }
/*
        [JsonProperty("order.pick_date")]
        public string? PickDate { get; set; }

        [JsonProperty("order.deliver_date")]
        public string? DeliverDate { get; set; }*/

       /* [JsonProperty("order.pick_money")]
        public int PickMoney { get; set; }*/

        [JsonProperty("order.note")]
        public required string Note { get; set; }

        [JsonProperty("order.value")]
        public required int Value { get; set; } //Giá trị đóng bảo hiểm, là căn cứ để tính phí bảo hiểm và bồi thường khi có sự cố.

       /* [JsonProperty("order.transport")]
        public required string Transport { get; set; }*/

        /*[JsonProperty("order.pick_option")]
        public required string PickOption { get; set; }*/

       /* [JsonProperty("order.deliver_option")]
        public string? DeliverOption { get; set; }*/

        [JsonProperty("order.tags")]
        public string[]? Tags { get; set; }

        [JsonProperty("products")]
        public required List<Product> Products { get; set; }
    }

    public class Product
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("weight")]
        public required decimal Weight { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        /*[JsonProperty("product_code")]
        public string? ProductCode { get; set; }*/ // Có thể là int trong JSON, nhưng để string cho linh hoạt
    }
}