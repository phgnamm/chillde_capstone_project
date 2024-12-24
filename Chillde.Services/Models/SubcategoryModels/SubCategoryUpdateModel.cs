using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.SubcategoryModels
{
    public class SubCategoryUpdateModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}
