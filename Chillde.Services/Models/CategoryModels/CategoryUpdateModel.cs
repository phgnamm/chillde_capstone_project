using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.CategoryModels
{
    public class CategoryUpdateModel
    {
        public string? Name { get; set; }
        public Guid? ParentId { get; set; }
        public string? AttachmentAlt { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}
