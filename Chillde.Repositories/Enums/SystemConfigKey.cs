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
        MaximumFeatureOfOnePackage
    }

}
