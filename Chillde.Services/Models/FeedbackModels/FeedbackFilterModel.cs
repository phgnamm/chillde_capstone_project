using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.FeedbackModels
{
    public class FeedbackFilterModel : FilterParameter
    {
        public bool? OneStar { get; set; }
        public bool? TwoStar { get; set; }
        public bool? ThreeStar { get; set; }
        public bool? FourStar { get; set; }
        public bool? FiveStar { get; set; }
    }
}
