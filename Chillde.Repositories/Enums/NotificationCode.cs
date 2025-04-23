namespace Chillde.Repositories.Enums
{
    public enum NotificationCode
    {
        Artisan_NewOrder,
        Artisan_AcceptSketch,
        Artisan_RejectSketch,
        Artisan_NewFeedback,
        Artisan_ReportOrder,
        Artisan_AcceptReport,
        Artisan_RejectReport,
        Artisan_CancelOrderDueToUnprocessedShipment,
        Artisan_AcceptDelivery,

        Customer_AcceptOrder,
        Customer_RejectOrder,
        Customer_NewSketch,        
        Customer_NewDelivery,
        Customer_InDelivery,
        Customer_Delivered,    
        Customer_AcceptReport,
        Customer_RejectReport,
        Customer_CancelOrderDueToUnprocessedShipment,
    }
}
