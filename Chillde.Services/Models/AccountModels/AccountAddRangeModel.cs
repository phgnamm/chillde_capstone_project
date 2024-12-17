using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.AccountModels;

public class AccountAddRangeModel
{
    [Required] public List<AccountSignUpModel> Accounts { get; set; } = null!;
}