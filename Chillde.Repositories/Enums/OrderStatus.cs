namespace Chillde.Repositories.Enums
{
    public enum OrderStatus
    {
        Pending = 0,      // Order has been created but not yet processed
        Processing = 1,   // Order is being processed
        Shipped = 2,      // Order has been shipped
        Delivered = 3,    // Order has been delivered
        Cancelled = 4,    // Order was cancelled
        Returned = 5,     // Order has been returned
        Failed = 6        // Order failed to complete
    }
}