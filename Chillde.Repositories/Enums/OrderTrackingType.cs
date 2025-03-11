namespace Chillde.Repositories.Enums;

public enum OrderTrackingType
{
    None, //(tin nhắn thông thường)
    Sketch, //(artisan yêu cầu review bản thảo)
    Delivery, //(artisan yêu cầu review delivery)
    Cancellation, //(một trong 2 bên yêu cầu hủy order)
    Shipping, //(yêu cầu shipping)
    DeadlineExpand, //(yêu cầu expand dl)
}