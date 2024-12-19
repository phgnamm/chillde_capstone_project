namespace Chillde.Repositories.Enums
{
    public enum OfferStatus
    {
        Pending = 0,      // Offer is created but not yet approved
        Approved = 1,     // Offer is approved
        Rejected = 2,     // Offer is rejected
        Active = 3,       // Offer is currently active
        Expired = 4,      // Offer has expired
        Cancelled = 5,    // Offer was cancelled
        Completed = 6     // Offer has been successfully completed
    }
}