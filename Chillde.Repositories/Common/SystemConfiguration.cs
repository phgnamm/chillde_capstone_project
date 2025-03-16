using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Common
{
    public class SystemConfiguration
    {
        public static readonly Dictionary<SystemConfigKey, string> ConfigKeys = new()
        {
            { SystemConfigKey.AccessTokenValidity, "AccessTokenValidity" },
            { SystemConfigKey.RefreshTokenValidity, "RefreshTokenValidityInDays" },
            { SystemConfigKey.ResetPasswordTokenValidity, "ResetPasswordTokenValidityInMinutes" },
            { SystemConfigKey.VerificationCodeLength, "VerificationCodeLength" },
            { SystemConfigKey.VerificationCodeValidity, "VerificationCodeValidityInMinutes" },
            { SystemConfigKey.ConversationMaxPageSize, "ConversationMaxPageSize" },
            { SystemConfigKey.DefaultMaxPageSize, "DefaultMaxPageSize" },
            { SystemConfigKey.DefaultMinPageSize, "DefaultMinPageSize" },
            { SystemConfigKey.MessageMaxPageSize, "MessageMaxPageSize" },
            { SystemConfigKey.MessageMinPageSize, "MessageMinPageSize" },
            { SystemConfigKey.DefaultAbsoluteExpiration, "DefaultAbsoluteExpirationInMinutes" },
            { SystemConfigKey.DefaultSlidingExpiration, "DefaultSlidingExpirationInMinutes" },
            { SystemConfigKey.MaximumPackageOfOneService, "MaximumPackageOfOneService" },
            { SystemConfigKey.MaximumFeatureOfOnePackage, "MaximumFeatureOfOnePackage" },
            // related to search history
            { SystemConfigKey.MaxSearchHistory , "MaxSearchHistory" },
            // related to voucher
            // configuration for create voucher in background service
            { SystemConfigKey.MinOrdersForArtisan , "MinOrdersForArtisan" },
            { SystemConfigKey.NumberOfMonthForMinOrders , "NumberOfMonthForMinOrders " },
            { SystemConfigKey.DiscountValue , "DiscountValue " },
            { SystemConfigKey.MaxDiscountValue , "MaxDiscountValue " },
            { SystemConfigKey.TotalQuantity , "TotalQuantity " },
            { SystemConfigKey.NumberOfDateForUsingVoucher , "NumberOfDateForUsingVoucher " },
            { SystemConfigKey.MinOrderValue , "MinOrderValue " },
            { SystemConfigKey.Commission, "Commission" },
            //New values ​​related to order processing
            { SystemConfigKey.MaxSystemCancelPerYear, "MaxSystemCancelPerYear" }, // Số lần hủy tối đa trong 1 năm
            { SystemConfigKey.MaxSystemCancelBeforePenalty, "MaxSystemCancelBeforePenalty" }, // Số lần hủy trước khi bị phạt
            { SystemConfigKey.PenaltyPercentageAfterCancel, "PenaltyPercentageAfterCancel" }, // % phạt sau khi vượt ngưỡng hủy
            { SystemConfigKey.OrderSuccessThreshold, "OrderSuccessThreshold" }, // Số đơn hàng thành công để reset điểm uy tín
            { SystemConfigKey.MaxOrderPerMonthBasedOnReputation, "MaxOrderPerMonthBasedOnReputation" }, // Giới hạn đơn hàng theo điểm uy tín
            { SystemConfigKey.MinReputationForVouchers, "MinReputationForVouchers" }, // Điểm tối thiểu để cấp voucher
            { SystemConfigKey.MaxCustomerOrdersPerMonth, "MaxCustomerOrdersPerMonth" }, // Giới hạn đơn hàng khách hàng trong tháng
            { SystemConfigKey.ReputationIncreaseOnSuccess, "ReputationIncreaseOnSuccess" }, // Điểm cộng khi đơn thành công
            { SystemConfigKey.MinReputationToAvoidBan, "MinReputationToAvoidBan" }, // Điểm tối thiểu để tránh bị ban
        };
    }
}
