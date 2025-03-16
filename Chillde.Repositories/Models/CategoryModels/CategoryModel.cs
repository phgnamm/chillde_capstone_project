using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.CategoryModels
{
    public class CategoryModel : BaseEntity
    {
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public Guid? ParentId { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }

    }
}
