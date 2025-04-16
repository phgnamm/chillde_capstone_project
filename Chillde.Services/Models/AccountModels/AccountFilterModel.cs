using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.AccountModels;

public class AccountFilterModel : FilterParameter
{
    public Gender? Gender { get; set; }
    public Role? Role { get; set; }
    public AccountStatus? Status { get; set; }

    // protected override int MinPageSize { get; set; } = Constant.;
    // protected override int MaxPageSize { get; set; } = Constant.;
}