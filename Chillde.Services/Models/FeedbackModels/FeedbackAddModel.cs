using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Services.Models.ServiceAttachmentModels;

namespace Chillde.Services.Models.FeedbackModels
{
    public class FeedbackAddModel
    {
        public required int Rating { get; set; }
        [StringLength(128, MinimumLength = 1)]
        public string? Description { get; set; }
        public Guid ServiceId { get; set; }
        public List<AttachmentAddModel>? Attachments { get; set; }

        // public ICollection<FeedbackAttachmentAddModel> FeedbackAttachmentAddModels { get; set; } = new List<FeedbackAttachmentAddModel>();
    }
    public class FeedbackAttachmentAddModel
    {
        public IFormFile? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }
    }
}
