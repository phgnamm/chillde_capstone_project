using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Enums
{
    public enum SystemConfigKey
    {
        AccessTokenValidity,
        RefreshTokenValidity,
        ResetPasswordTokenValidity,
        VerificationCodeLength,
        VerificationCodeValidity,
        ConversationMaxPageSize,
        DefaultMaxPageSize,
        DefaultMinPageSize,
        MessageMaxPageSize,
        MessageMinPageSize,
        DefaultAbsoluteExpiration,
        DefaultSlidingExpiration,
        MaximumPackageOfOneService,
        MaximumPackageFeatureOfOnePackage,
        MaximumFeatureOfOnePackage,
        // percent order for cancellation automatically
        AutoCancelPercentagePenalty,
        AutoCancelPointPenalty,
        // related to search history
        MaxSearchHistory,

        // related to voucher
        // dieu kien de tao voucher cho nghe nhan, so order toi thieu trong bao nhieu thang
        MinOrdersForArtisan,
        NumberOfMonthForMinOrders,
        MinReputationOfArtisan,

        // gia tri discount cho voucher
        DiscountValue,

        // gia tri maxDiscount
        MaxDiscountValue,

        // so luong voucher
        TotalQuantity,

        // so ngay ket thuc voucher
        NumberOfDateForUsingVoucher,

        // toi thieu don hang 
        MinOrderValue,
        Commission,

        //New values ​​related to order processing
        MaxSystemCancelPerYear,
        MaxSystemCancelBeforePenalty,
        PenaltyPercentageAfterCancel,
        OrderSuccessThreshold,
        MaxOrderPerMonthBasedOnReputation,
        MinReputationForVouchers,
        MaxCustomerOrdersPerMonth,
        ReputationIncreaseOnSuccess,
        MinReputationToAvoidBan,
        MaxPriceOfPackage
    }
}