using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.CategoryModels
{
    public class CategoryAddRangeModel
    {
        public required List<CategoryAddRequestModel> CategoryAddRequestModels { get; set; }
        public List<IFormFile>? ImageUrls { get; set; }    
    }
    public class CategoryAddRequestModel
    {
        public required string Name { get; set; }
        public string? Code { get; set; }
    }
}
