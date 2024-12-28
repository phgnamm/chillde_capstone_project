namespace Chillde.Repositories.Enums;

public enum OrderStatus
{
    Pending , // Order has been created but not yet processed
    Success, // Order has been created 
    Processing, // Order is being processed
    Shipped , // Order has been shipped
    Delivered , // Order has been delivered
    Cancelled , // Order was cancelled
    Returned , // Order has been returned
    Failed  // Order failed to complete
}