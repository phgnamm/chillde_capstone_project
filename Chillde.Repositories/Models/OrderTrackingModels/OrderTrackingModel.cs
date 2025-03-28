using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Models.OrderTrackingModels
{
    public class OrderTrackingModel : BaseEntity
    {
        public string? CreatedBy { get; set; }
        public string? CreatedRole { get; set; }
        public int? CurrentSketchRevision { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsAccepted { get; set; } = null;
        public OrderStage? Stage { get; set; }
        public OrderTrackingType? Type { get; set; }
        public ICollection<OrderTrackingAttachmentModel>? OrderTrackingAttachmentModels { get; set; } = new List<OrderTrackingAttachmentModel>();
    }
    public class OrderTrackingAttachmentModel
    {
        public Guid Id { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }
    }
}
