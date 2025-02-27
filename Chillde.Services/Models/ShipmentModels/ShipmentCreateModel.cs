using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ShipmentModels
{
    public class ShipmentCreateModel
    {
        public required string PickName { get; set; } // Tên người gửi
        public required string PickAddress { get; set; } // Địa chỉ lấy hàng
        public required string PickProvince { get; set; } // Tỉnh lấy hàng
        public required string PickDistrict { get; set; } // Quận lấy hàng
        public required string PickWard { get; set; } // Phường lấy hàng
        public required string PickTel { get; set; } // SĐT người gửi

        public required string ReceiverName { get; set; } // Tên người nhận
        public required string ReceiverAddress { get; set; } // Địa chỉ người nhận
        public required string ReceiverProvince { get; set; }
        public required string ReceiverDistrict { get; set; }
        public required string ReceiverWard { get; set; }
        public required string ReceiverTel { get; set; }
        public required string ReceiverHamlet { get; set; }

        public bool IsFreeShip { get; set; } // Có miễn phí ship không
        public DateTime PickDate { get; set; } // Ngày lấy hàng
        public decimal PickMoney { get; set; } // Tiền thu hộ (COD)
        public required string Note { get; set; } // Ghi chú
        public decimal Value { get; set; } // Giá trị đơn hàng
        public required string Transport { get; set; } // Loại vận chuyển (fly, xteam,...)
        public required string PickOption { get; set; } // Tùy chọn lấy hàng (cod,...)
        public required string DeliverOption { get; set; } // Tùy chọn giao hàng
            
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
