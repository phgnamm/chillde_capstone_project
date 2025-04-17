using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Role = Chillde.Repositories.Enums.Role;

namespace Chillde.Repositories.Models.AccountRoleModels;

public class AccountRoleModel : BaseEntity
{
    public int TotalReputation { get; set; }
    public AccountStatus Status { get; set; }
    public Role Role { get; set; }
    public string RoleName { get; set; } = null!;
}