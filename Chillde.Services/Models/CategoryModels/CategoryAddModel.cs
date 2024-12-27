using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.CategoriesModels
{
    public class CategoryAddModel
    {    
        public required string Name { get; set; }
        public string? Code { get; set; }      
        public IFormFile? ImageUrl { get; set; }
    }
}
