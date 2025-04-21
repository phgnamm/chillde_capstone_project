using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Common
{
    public class NotificationTypeCode
    {
        public static readonly Dictionary<NotificationCode, string> NotiCodes = new()
        {
            { NotificationCode.AcceptOrder, "ORD_01" },
            { NotificationCode.RejectOrder, "ORD_02" },
            { NotificationCode.NewOrder, "ORD_03" },
            { NotificationCode.NewSketch, "ORD_TRC_01" },
            { NotificationCode.AcceptSketch, "ORD_TRC_02" },
            { NotificationCode.RejectSketch, "ORD_TRC_03" },
            { NotificationCode.NewDelivery, "ORD_TRC_04" },
            { NotificationCode.InDelivery, "ORD_TRC_05" },
            { NotificationCode.Delivered, "ORD_TRC_06" },
            { NotificationCode.NewFeedback, "FEEDBACK_01" },
            { NotificationCode.ReportOrder, "REP_01" },
            { NotificationCode.AcceptReport, "REP_02" },
            { NotificationCode.RejectReport, "REP_03" },
        };
    }
}
