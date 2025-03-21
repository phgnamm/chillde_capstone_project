using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Models.OrderTrackingModels
{
    public class OrderTrackingAddModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public OrderStage Stage { get; set; }
        public OrderTrackingType Type { get; set; }
        public ICollection<OrderTrackingAttachmentAddModel>? OrderTrackingAttachmentAddModels { get; set; } = new List<OrderTrackingAttachmentAddModel>();

    }
    public class OrderTrackingAttachmentAddModel
    {
        public string? AttachmentAlt { get; set; }
        public IFormFile? AttachmentUrl { get; set; }
    }
}
