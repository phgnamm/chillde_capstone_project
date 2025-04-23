using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Common
{
    public class NotificationTypeCode
    {
        public static readonly Dictionary<NotificationCode, string> NotiCodes = new()
        {
            { NotificationCode.Customer_AcceptOrder, "ORD_01" },
            { NotificationCode.Customer_RejectOrder, "ORD_02" },
            { NotificationCode.Artisan_NewOrder, "ORD_03" },
            { NotificationCode.Customer_NewSketch, "ORD_TRC_01" },
            { NotificationCode.Artisan_AcceptSketch, "ORD_TRC_02" },
            { NotificationCode.Artisan_RejectSketch, "ORD_TRC_03" },
            { NotificationCode.Customer_NewDelivery, "ORD_TRC_04" },
            { NotificationCode.Customer_InDelivery, "ORD_TRC_05" },
            { NotificationCode.Customer_Delivered, "ORD_TRC_06" },
            { NotificationCode.Artisan_NewFeedback, "FEEDBACK_01" },
            { NotificationCode.Artisan_ReportOrder, "REP_01" },
            { NotificationCode.Artisan_AcceptReport, "REP_02" },
            { NotificationCode.Customer_RejectReport, "REP_03" },
            { NotificationCode.Artisan_RejectReport, "REP_04" },
            { NotificationCode.Customer_AcceptReport, "REP_05" },
        };
    }
}
