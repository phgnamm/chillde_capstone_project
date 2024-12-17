using System.ComponentModel.DataAnnotations;
using Chillde.Repositories.Enums;

namespace Chillde.Services.Models.AccountModels;

public class AccountUpdateRolesModel
{
    [Required] public List<Role> Roles { get; set; } = null!;
}