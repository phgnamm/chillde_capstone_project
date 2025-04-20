using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.AccountModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.FeedbackModels
{
    public class FeedbackModel : BaseEntity
    {
        public int? Rating { get; set; }
        public string? Description { get; set; }
        public required AccountLiteModel CreatedBy { get; set; }
        public Guid? ServiceId { get; set; }
        public string ? Response { get; set; }
        public ICollection<FeedbackAttachmentModel> FeedbackAttachmentModels { get; set; } = new List<FeedbackAttachmentModel>();
    }
    public class FeedbackAttachmentModel
    {
        public string? AttachmentAlt { get; set; }
        public string? AttachmentUrl { get; set; }
    }

}
