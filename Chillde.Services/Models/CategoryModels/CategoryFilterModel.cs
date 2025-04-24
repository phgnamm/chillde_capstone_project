using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.CategoryModels
{
    public class CategoryFilterModel : FilterParameter
    {
        public Guid? ParentId { get; set; } 
        public string? Slug { get; set; } 
        public bool IncludeChildren { get; set; }
        protected override int MinPageSize { get; set; } = 2;
        protected override int MaxPageSize { get; set; } = 1000;
    }
}   

