namespace Chillde.Repositories.Enums
{
    public enum WalletHistoryStatus
    {
        Pending = 0,      // Transaction is pending and has not been processed yet
        Completed = 1,    // Transaction has been successfully completed
        Failed = 2,       // Transaction failed due to some issue
        Reversed = 3,     // Transaction was reversed or refunded
        Canceled = 4,     // Transaction was canceled
        InProcess = 5     // Transaction is being processed
    }
}