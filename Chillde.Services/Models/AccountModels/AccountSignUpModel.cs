using System.ComponentModel.DataAnnotations;
using Chillde.Repositories.Enums;
using Chillde.Services.Models.AccountModels.Validations;

namespace Chillde.Services.Models.AccountModels;

public class AccountSignUpModel
{
    [Required] [StringLength(50)] public string FirstName { get; set; } = null!;
    [Required] [StringLength(50)] public string LastName { get; set; } = null!;

    [Required]
    [RegularExpression(@"^[a-zA-Z0-9_]*$",
        ErrorMessage = "Username can only contain alphanumeric characters and underscores.")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [EmailAddress] [StringLength(256)] public string Email { get; set; } = null!;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = null!;

    [EnumDataType(typeof(Gender))] public Gender? Gender { get; set; }

    [DateOfBirthValidation] public DateOnly? DateOfBirth { get; set; }
    [Phone] [StringLength(15)] public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public List<Role>? Roles { get; set; } = null!;
}