using Chillde.Services.Models.ResponseModels;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.AccountModels.Validations;

public class DateOfBirthValidation : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateOnly date)
            if (date > DateOnly.FromDateTime(DateTime.Now))
                return new ValidationResult("Date of birth cannot be in the future");

        return ValidationResult.Success;
    }
}