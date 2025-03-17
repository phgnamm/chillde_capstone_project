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
        public string? Slug { get; set; }
        public Guid? CategoryId { get; set; }
    }
}

