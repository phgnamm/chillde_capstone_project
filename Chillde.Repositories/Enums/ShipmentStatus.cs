namespace Chillde.Repositories.Enums
{
    public enum ShipmentStatus
    {
        Pending = 0,      // Shipment has been created but not yet processed
        Shipped = 1,      // Shipment has been dispatched and is on its way
        InTransit = 2,    // Shipment is currently in transit
        OutForDelivery = 3, // Shipment is out for delivery to the destination
        Delivered = 4,    // Shipment has been successfully delivered
        FailedDelivery = 5, // Delivery attempt failed
        Returned = 6,     // Shipment is being returned to the sender
        Cancelled = 7     // Shipment has been cancelled
    }
}