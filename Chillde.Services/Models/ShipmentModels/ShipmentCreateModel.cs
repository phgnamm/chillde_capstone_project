using Newtonsoft.Json;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Chillde.Services.Models.ShipmentModels
{
    public class ShipmentCreateModel
    {
        #region Artisan's information
        [JsonProperty("order.pick_name")]
        public required string PickName { get; set; } // Tên người gửi

        [JsonProperty("order.pick_address")]
        public required string PickAddress { get; set; } // Địa chỉ lấy hàng

        [JsonProperty("order.pick_province")]
        public required string PickProvince { get; set; } // Tỉnh lấy hàng

        [JsonProperty("order.pick_district")]
        public required string PickDistrict { get; set; } // Quận lấy hàng

        [JsonProperty("order.pick_ward")]
        public required string PickWard { get; set; } // Phường lấy hàng

        [JsonProperty("order.pick_tel")]
        public required string PickTel { get; set; } // SĐT người gửi
        #endregion

        #region Customer's information
        [JsonProperty("order.name")]
        public required string Name { get; set; } // Tên người nhận

        [JsonProperty("order.address")]
        public required string Address { get; set; } // Địa chỉ người nhận

        [JsonProperty("order.province")]
        public required string Province { get; set; }

        [JsonProperty("order.district")]
        public required string District { get; set; }

        [JsonProperty("order.ward")]
        public required string Ward { get; set; }

        [JsonProperty("order.tel")]
        public required string Tel { get; set; }

        [JsonProperty("order.hamlet")]
        public required string Hamlet { get; set; } //Tên thôn/ấp/xóm/tổ/... của người nhận hàng hóa.
                                                    //Nếu không có, vui lòng điền "Khác"
        [JsonProperty("order.email")]
        public required string Email { get; set; }
        #endregion

        #region Return information
        //[JsonProperty("order.return_name")]
        //public required string ReturnName { get; set; } // Tên người nhận trả hàng

        //[JsonProperty("order.return_address")]
        //public required string ReturnAddress { get; set; } // Địa chỉ trả hàng

        //[JsonProperty("order.return_province")]
        //public required string ReturnProvince { get; set; } // Tỉnh trả hàng

        //[JsonProperty("order.return_district")]
        //public required string ReturnDistrict { get; set; } // Quận trả hàng

        //[JsonProperty("order.return_tel")]
        //public required string ReturnTel { get; set; } // Số điện thoại người nhận hàng hóa

        //[JsonProperty("order.return_email")]
        //public required string ReturnEmail { get; set; } // Email người nhận hàng hóa
        #endregion

        [JsonProperty("order.is_freeship")]
        public int IsFreeShip { get; set; } // Có miễn phí ship không

        [JsonProperty("order.pick_date")]
        public string? PickDate { get; set; } // Ngày lấy hàng

        [JsonProperty("order.deliver_date")]
        public string? DeliverDate { get; set; } // Ngày giao hàng

        [JsonProperty("order.pick_money")]
        public int PickMoney { get; set; } // Tiền thu hộ (COD)

        [JsonProperty("order.note")]
        public required string Note { get; set; } // Ghi chú

        [JsonProperty("order.value")]
        public required int Value { get; set; } // Giá trị đơn hàng dùng để bồi thường

        [JsonProperty("order.transport")]
        public required string Transport { get; set; } // Loại vận chuyển (fly, road)

        [JsonProperty("order.pick_option")]
        public required string PickOption { get; set; } // Tùy chọn lấy hàng (cod,...)

        [JsonProperty("order.deliver_option")]
        public string? DeliverOption { get; set; } // Tùy chọn giao hàng

        [JsonProperty("order.tags")]
        public string[]? Tags { get; set; } // Gắn nhãn cho đơn hàng,
                                            // xem các nhãn hỗ trợ tại đây:
                                            // https://docs.giaohangtietkiem.vn/docs/submit-order/submit-order-express

        [JsonProperty("products")]
        public required List<Product> Products { get; set; } // Danh sách sản phẩm
    }

    public class Product
    {
        [JsonProperty("name")]
        public required string Name { get; set; } // Tên sản phẩm

        [JsonProperty("weight")]
        public required decimal Weight { get; set; } // Khối lượng (kg)

        [JsonProperty("quantity")]
        public int? Quantity { get; set; } // Số lượng

        [JsonProperty("product_code")]
        public string? ProductCode { get; set; } // Mã sản phẩm
    }
}
