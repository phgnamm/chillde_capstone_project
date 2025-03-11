using Newtonsoft.Json;

namespace Chillde.Repositories.Models.ShipmentModels
{
    public class ShipmentResponseModel
    {
        
        [JsonProperty("code")]
        public string? Code { get; set; }
        [JsonProperty("message")]
        public string? Message { get; set; }
        [JsonProperty("success")]
        public string? Success { get; set; }

    }
    public class ShipmentAddResponseModel : ShipmentResponseModel
    {
        [JsonProperty("error_code")]
        public string? ErrorCode { get; set; }
        [JsonProperty("order")]
        public OrderInfo? Order { get; set; }
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
/*
        public class ShipmentAddResponseModel
        {
            [JsonProperty("partner_id")]
            public string? PartnerId { get; set; } // Mã đơn hàng nội bộ của đối tác

            [JsonProperty("label")]
            public string? Label { get; set; } // Mã vận đơn GHTK

            [JsonProperty("area")]
            public int? Area { get; set; } // Khu vực (số nguyên)

            [JsonProperty("fee")]
            public int? Fee { get; set; } // Phí vận chuyển

            [JsonProperty("insurance_fee")]
            public int? InsuranceFee { get; set; } // Phí bảo hiểm

            [JsonProperty("estimated_pick_time")]
            public string? EstimatedPickTime { get; set; } // Thời gian dự kiến lấy hàng

            [JsonProperty("estimated_deliver_time")]
            public string? EstimatedDeliverTime { get; set; } // Thời gian dự kiến giao hàng

            [JsonProperty("products")]
            public List<ProductResponse>? Products { get; set; } // Danh sách sản phẩm (trong trường hợp này là mảng rỗng)

            [JsonProperty("status_id")]
            public int? StatusId { get; set; } // Mã trạng thái đơn hàng

            [JsonProperty("tracking_id")]
            public long? TrackingId { get; set; } // ID theo dõi

            [JsonProperty("sorting_code")]
            public string? SortingCode { get; set; } // Mã phân loại tuyến

            [JsonProperty("date_to_delay_pick")]
            public string? DateToDelayPick { get; set; } // Ngày trì hoãn lấy hàng

            [JsonProperty("pick_work_shift")]
            public int? PickWorkShift { get; set; } // Ca làm việc lấy hàng

            [JsonProperty("date_to_delay_deliver")]
            public string? DateToDelayDeliver { get; set; } // Ngày trì hoãn giao hàng

            [JsonProperty("deliver_work_shift")]
            public int? DeliverWorkShift { get; set; } // Ca làm việc giao hàng

            [JsonProperty("pkg_draft_id")]
            public int? PkgDraftId { get; set; } // ID bản nháp gói hàng

            [JsonProperty("package_id")]
            public string? PackageId { get; set; } // ID gói hàng (UUID)

            [JsonProperty("cost_id")]
            public string? CostId { get; set; } // ID chi phí (có thể null)

            [JsonProperty("is_xfast")]
            public int? IsXfast { get; set; } // Có phải dịch vụ Xfast không (0 hoặc 1)
        }

        // Lớp phụ cho sản phẩm (nếu có dữ liệu trong tương lai)
        public class ProductResponse
        {
            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("weight")]
            public decimal? Weight { get; set; }

            [JsonProperty("quantity")]
            public int? Quantity { get; set; }

            [JsonProperty("product_code")]
            public string? ProductCode { get; set; }
        }*/
    
}
