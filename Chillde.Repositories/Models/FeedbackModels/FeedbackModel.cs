using Chillde.Repositories.Entities;
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
        public string AuthorName { get; set; }
        public Guid? ServiceId { get; set; }
        public string ? Response { get; set; }
        public ICollection<FeedbackImageModel> FeedbackImageModels { get; set; } = new List<FeedbackImageModel>();
    }
    public class FeedbackImageModel
    {
        public string ImageUrl { get; set; }
    }

}
