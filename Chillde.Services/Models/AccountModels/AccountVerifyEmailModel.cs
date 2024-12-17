using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.AccountModels;

public class AccountVerifyEmailModel
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required] public string VerificationCode { get; set; } = null!;
}