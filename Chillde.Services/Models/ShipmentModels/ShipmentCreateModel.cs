using Newtonsoft.Json;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Chillde.Services.Models.ShipmentModels
{
    public class ShipmentCreateModel
    {
        #region Artisan's information
        public required string PickName { get; set; } // Tên người gửi
        public required string PickAddress { get; set; } // Địa chỉ lấy hàng
        public required string PickProvince { get; set; } // Tỉnh lấy hàng
        public required string PickDistrict { get; set; } // Quận lấy hàng
        public required string PickWard { get; set; } // Phường lấy hàng
        public required string PickTel { get; set; } // SĐT người gửi
        #endregion

        #region Customer's information
        public required string Name { get; set; } // Tên người nhận
        public required string Address { get; set; } // Địa chỉ người nhận
        public required string Province { get; set; }
        public required string District { get; set; }
        public required string Ward { get; set; }
        public required string Tel { get; set; }
        public required string Hamlet { get; set; } //Tên thôn/ấp/xóm/tổ/... của người nhận hàng hóa.
                                                    //Nếu không có, vui lòng điền "Khác"
        public required string Email { get; set; }
        #endregion

        #region Return information
        public required string ReturnName { get; set; } // Tên người nhận trả hàng
        public required string ReturnAddress { get; set; } // Địa chỉ trả hàng
        public required string ReturnProvince { get; set; } // Tỉnh trả hàng
        public required string ReturnDistrict { get; set; } // Quận trả hàng
        public required string ReturnTel { get; set; } // Số điện thoại người nhận hàng hóa
        public required string ReturnHEmail { get; set; } // Email người nhận hàng hóa
        #endregion

        public int IsFreeShip { get; set; } // Có miễn phí ship không
        public string? PickDate { get; set; } // Ngày lấy hàng
        public string? DeliverDate { get; set; } // Ngày giao hàng
        public int PickMoney { get; set; } // Tiền thu hộ (COD)
        public required string Note { get; set; } // Ghi chú
        public required int Value { get; set; } // Giá trị đơn hàng dùng để bồi thường
        public required string Transport { get; set; } // Loại vận chuyển (fly, road)
        public required string PickOption { get; set; } // Tùy chọn lấy hàng (cod,...)
        public string? DeliverOption { get; set; } // Tùy chọn giao hàng
        public string[]? Tags { get; set; } //Gắn nhãn cho đơn hàng,
                                                    //xem các nhãn hỗ trợ tại đây:
                                                    //https://docs.giaohangtietkiem.vn/docs/submit-order/submit-order-express

        public required List<Product> Products { get; set; } // Danh sách sản phẩm
    }

    public class Product
    {
        public required string Name { get; set; } // Tên sản phẩm
        public decimal Weight { get; set; } // Khối lượng (kg)
        public int Quantity { get; set; } // Số lượng
        public required string ProductCode { get; set; } // Mã sản phẩm
    }
}
