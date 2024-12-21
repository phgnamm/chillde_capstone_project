using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.FeedbackModels
{
    public class FeedbackAddModel
    {
        public required int Rating { get; set; }
        [Length(128, 1)]
        public string? Description { get; set; }
        public Guid ServiceId { get; set; }
        public ICollection<FeedbackImageAddModel> FeedbackImageAddModels { get; set; } = new List<FeedbackImageAddModel>();
    }
    public class FeedbackImageAddModel
    {
        public IFormFile? ImageUrl { get; set; }
    }
}
