using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.ItemModels
{
    public class ItemModelById : BaseEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public Guid? CategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public Guid SubCategoryId { get; set; }
    }
}
