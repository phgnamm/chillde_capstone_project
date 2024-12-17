namespace Chillde.Repositories.Enums
{
    public enum WalletHistoryType
    {
        Deposit = 0,      // Money was deposited into the wallet
        Withdrawal = 1,   // Money was withdrawn from the wallet
        TransferIn = 2,   // Funds were transferred into the wallet from another account
        TransferOut = 3,  // Funds were transferred out of the wallet to another account
        Adjustment = 4,   // A manual adjustment was made to the wallet balance
    }
}