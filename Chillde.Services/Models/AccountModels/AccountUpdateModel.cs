using System.ComponentModel.DataAnnotations;
using Chillde.Repositories.Enums;
using Chillde.Services.Models.AccountModels.Validations;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Models.AccountModels;

public class AccountUpdateModel
{
    [Required] [StringLength(50)] public string FirstName { get; set; } = null!;
    [Required] [StringLength(50)] public string LastName { get; set; } = null!;

    [Required]
    [RegularExpression(@"^[a-zA-Z0-9_]*$",
        ErrorMessage = "Username can only contain alphanumeric characters and underscores.")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [EnumDataType(typeof(Gender))] public Gender? Gender { get; set; }
    [DateOfBirthValidation] public DateOnly? DateOfBirth { get; set; }
    [Phone] [StringLength(15)] public string? PhoneNumber { get; set; }
    public string? StoreAddress { get; set; }
    public string? StoreDescription { get; set; }
    public string? Image { get; set; }
    public string? Banner { get; set; }
    // public IFormFile? NewImage { get; set; }
    // public IFormFile? NewBanner { get; set; }
}