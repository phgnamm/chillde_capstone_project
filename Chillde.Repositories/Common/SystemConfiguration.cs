using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Common
{
    public class SystemConfiguration
    {
        public static readonly Dictionary<SystemConfigKey, string> ConfigKeys = new()
        {
            { SystemConfigKey.AccessTokenValidity, "AccessTokenValidityInMinutes" },
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
            { SystemConfigKey.DefaultSlidingExpiration, "DefaultSlidingExpirationInMinutes" }
        };
    }
}
