using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.CategoriesModels
{
    public class  CategoryAddModel
    {    
        public required string Name { get; set; }
        public Guid? ParentId { get; set; }
        public IFormFile? ImageUrl { get; set; }
        public string? AttachmentAlt { get; set; }
    }
}
