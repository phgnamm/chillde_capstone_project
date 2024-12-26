using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.RequestModels
{
    public class RequestAddModel
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Decimal MinBudget { get; set; }
        public required Decimal MaxBudget { get; set; }
        public int Timeline { get; set; }
        public IFormFile? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }
        public required Guid ItemId { get; set; }
        public required Guid AccountId { get; set; }
        public ICollection<RequestDetailAddModel> RequestDetailAddModels { get; set; } = new List<RequestDetailAddModel>();

    }
    public class RequestDetailAddModel
    {
        public required string Description { get; set; }
        public required Guid AttributeId { get; set; }
    }
}
