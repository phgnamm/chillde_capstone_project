namespace Chillde.Repositories.Enums;

public enum OrderStatus
{
    Pending , // Order has been created but not yet processed
    Accepted, // Order has been accepted by the system
    Rejected, // Order has been rejected by the system
    Completed, // Order has been success
    Cancelled, // Order has been success
    Refunded // Order has been refunded
}