namespace Chillde.Repositories.Enums
{
    public enum OrderTrackingType
    {
        None = 0,          // No tracking information available
        Shipping = 1,      // Order is being tracked via shipping provider
        Delivery = 2,      // Order is being tracked for delivery status
        Payment = 3,       // Order is being tracked based on payment status
        Cancellation = 4,  // Order is being tracked during the cancellation process
    }
}