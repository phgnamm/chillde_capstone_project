namespace Chillde.Repositories.Enums;

public enum ShipmentStatus
{
    Cancelled = -1, // Hủy đơn hàng
    NotReceived = 1, // Chưa tiếp nhận
    Received = 2, // Đã tiếp nhận
    PickedUp = 3, // Đã lấy hàng/Đã nhập kho
    Delivering = 4, // Đã điều phối giao hàng/Đang giao hàng
    DeliveredNotReconciled = 5, // Đã giao hàng/Chưa đối soát
    Reconciled = 6, // Đã đối soát
    PickupFailed = 7, // Không lấy được hàng
    PickupDelayed = 8, // Hoãn lấy hàng
    DeliveryFailed = 9, // Không giao được hàng
    DeliveryDelayed = 10, // Delay giao hàng
    ReturnedReconciled = 11, // Đã đối soát công nợ trả hàng
    PickupArranging = 12, // Đã điều phối lấy hàng/Đang lấy hàng
    CompensationOrder = 13, // Đơn hàng bồi hoàn
    Returning = 20, // Đang trả hàng (COD cầm hàng đi trả)
    Returned = 21, // Đã trả hàng (COD đã trả xong hàng)

    ShipperPickedUp = 123, // Shipper báo đã lấy hàng
    ShipperPickupFailed = 127, // Shipper báo không lấy được hàng
    ShipperPickupDelayed = 128, // Shipper báo delay lấy hàng
    ShipperDelivered = 45, // Shipper báo đã giao hàng
    ShipperDeliveryFailed = 49, // Shipper báo không giao được hàng
    ShipperDeliveryDelayed = 410 // Shipper báo delay giao hàng
}