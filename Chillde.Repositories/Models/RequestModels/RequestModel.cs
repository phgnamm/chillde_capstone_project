using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.RequestModels
{
    public class RequestModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public int? Timeline { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }
}
