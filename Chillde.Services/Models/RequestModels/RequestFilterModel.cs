using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;

namespace Chillde.Services.Models.RequestModels
{
    public class RequestFilterModel : FilterParameter
    {
        public bool ViewAll { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public int? Timeline { get; set; }
        public RequestStatus? Status { get; set; }
    }
}
