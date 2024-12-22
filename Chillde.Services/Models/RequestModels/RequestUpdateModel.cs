using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.RequestModels
{
    public class RequestUpdateModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public int? Timeline { get; set; }
        public IFormFile? AttachmentUrl { get; set; }
        public Guid ItemId { get; set; }
        public List<RequestDetailUpdateModel>? RequestDetailUpdateModels { get; set; }
    }

    public class RequestDetailUpdateModel
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public Guid ItemAttributeId { get; set; }
    }
}
