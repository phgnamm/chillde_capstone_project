using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.DashBoardModels
{
    public class DashboardFilterModel
    {
        private static readonly int CurrentMonth = DateTime.Today.Month;
        private static readonly int CurrentYear = DateTime.Today.Year;

        [Range(1, 12, ErrorMessage = "AmountMonth must be between 1 and 12.")]
        public int Month { get; set; } = CurrentMonth;

        [CustomYearValidation(ErrorMessage = "CommentYear cannot be greater than the current year.")]
        public int Year { get; set; } = CurrentYear;
    }
}
