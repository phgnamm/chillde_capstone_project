using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.DashBoardModels
{
    public class CustomYearValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var year = (int)value;

            if (year > DateTime.Today.Year)
            {
                return new ValidationResult(ErrorMessage ?? "Year cannot be greater than the current year.");
            }

            return ValidationResult.Success;
        }
    }

}
