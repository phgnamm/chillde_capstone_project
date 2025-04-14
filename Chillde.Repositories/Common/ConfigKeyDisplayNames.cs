using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Common
{
    public class ConfigKeyDisplayNames
    {
        public static readonly Dictionary<SystemConfigKey, string> DisplayNames = new()
        {
            // Thời gian hiệu lực
            { SystemConfigKey.AccessTokenValidity, "Thời gian hiệu lực Access Token (phút)" },
            { SystemConfigKey.RefreshTokenValidity, "Thời gian hiệu lực Refresh Token (ngày)" },
            { SystemConfigKey.ResetPasswordTokenValidity, "Thời gian hiệu lực mã đặt lại mật khẩu (phút)" },
            { SystemConfigKey.VerificationCodeLength, "Độ dài mã xác thực" },
            { SystemConfigKey.VerificationCodeValidity, "Thời gian hiệu lực mã xác thực (phút)" },

            // Phân trang
            { SystemConfigKey.ConversationMaxPageSize, "Số mục tối đa trong 1 trang hội thoại" },
            { SystemConfigKey.DefaultMaxPageSize, "Số mục tối đa mặc định/trang" },
            { SystemConfigKey.DefaultMinPageSize, "Số mục tối thiểu mặc định/trang" },
            { SystemConfigKey.MessageMaxPageSize, "Số tin nhắn tối đa/trang" },
            { SystemConfigKey.MessageMinPageSize, "Số tin nhắn tối thiểu/trang" },

            // Thời gian hết hạn dữ liệu
            { SystemConfigKey.DefaultAbsoluteExpiration, "Thời gian hết hạn tuyệt đối (phút)" },
            { SystemConfigKey.DefaultSlidingExpiration, "Thời gian hết hạn trượt (phút)" },

            // Gói dịch vụ
            { SystemConfigKey.MaximumPackageOfOneService, "Số gói tối đa cho mỗi dịch vụ" },
            { SystemConfigKey.MaximumPackageFeatureOfOnePackage, "Số tính năng tối đa cho mỗi gói" },
            { SystemConfigKey.MaxPriceOfPackage, "Giá tối đa cho mỗi gói (VND)" },

            // Xử lý hủy đơn
            { SystemConfigKey.AutoCancelPercentagePenalty, "Phạt % khi tự động hủy đơn" },
            { SystemConfigKey.AutoCancelPointPenalty, "Trừ điểm khi tự động hủy đơn" },

            // Dịch vụ của nghệ nhân
            { SystemConfigKey.MaximumSerivceOfOneArtisan, "Số dịch vụ tối đa/nghệ nhân" },

            // Lịch sử tìm kiếm
            { SystemConfigKey.MaxSearchHistory, "Số lịch sử tìm kiếm tối đa" },

            // Voucher - Điều kiện tạo
            { SystemConfigKey.MinOrdersForArtisan, "Số đơn tối thiểu để cấp voucher" },
            { SystemConfigKey.NumberOfMonthForMinOrders, "Số tháng kiểm tra đơn tối thiểu" },
            { SystemConfigKey.MinReputationOfArtisan, "Điểm uy tín tối thiểu để nhận voucher" },

            // Voucher - Thông tin chính
            { SystemConfigKey.DiscountValue, "Giá trị giảm giá (%)" },
            { SystemConfigKey.MaxDiscountValue, "Mức giảm tối đa (VND)" },
            { SystemConfigKey.TotalQuantity, "Tổng số lượng voucher" },
            { SystemConfigKey.NumberOfDateForUsingVoucher, "Số ngày hiệu lực của voucher" },
            { SystemConfigKey.MinOrderValue, "Giá trị đơn hàng tối thiểu áp dụng voucher (VND)" },
            { SystemConfigKey.Commission, "Hoa hồng hệ thống (%)" },

            // Quản lý đơn hàng & uy tín
            { SystemConfigKey.MaxSystemCancelPerYear, "Số lần hệ thống cho phép hủy/năm" },
            { SystemConfigKey.MaxSystemCancelBeforePenalty, "Số lần hủy trước khi bị phạt" },
            { SystemConfigKey.PenaltyPercentageAfterCancel, "Phạt % sau khi vượt giới hạn hủy" },
            { SystemConfigKey.OrderSuccessThreshold, "Số đơn thành công để reset uy tín" },
            { SystemConfigKey.MaxOrderPerMonthBasedOnReputation, "Giới hạn đơn hàng/tháng theo uy tín" },
            { SystemConfigKey.MinReputationForVouchers, "Uy tín tối thiểu để nhận voucher" },
            { SystemConfigKey.MaxCustomerOrdersPerMonth, "Giới hạn đơn hàng khách hàng/tháng" },
            { SystemConfigKey.ReputationIncreaseOnSuccess, "Điểm cộng khi hoàn thành đơn thành công" },
            { SystemConfigKey.MinReputationToAvoidBan, "Điểm uy tín tối thiểu tránh bị khóa tài khoản" }
        };

    }
}
