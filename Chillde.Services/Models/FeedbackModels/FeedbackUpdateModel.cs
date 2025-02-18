using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.FeedbackModels
{
    public class FeedbackUpdateModel
    {
        public int? Rating { get; set; }
        public string? Description { get; set; }
        public string ? Response { get; set; }
       /* public List<FeedbackImageUpdateModel>? FeedbackImageUpdateModels { get; set; } = new List<FeedbackImageUpdateModel>();*/
    }

   /* public class FeedbackImageUpdateModel
    {
        public Guid? Id { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }*/

}
