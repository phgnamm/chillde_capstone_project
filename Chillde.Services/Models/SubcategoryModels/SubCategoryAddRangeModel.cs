using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.SubcategoryModels
{
    public class SubCategoryAddRangeModel
    {
        public required List<SubCategoryAddRequestModel> SubCategoryAddRequestModels { get; set; }
        public List<IFormFile>? ImageUrls { get; set; }
    }
    public class SubCategoryAddRequestModel
    {
        public required string Name { get; set; }
        public string? Code { get; set; }
    }
}
