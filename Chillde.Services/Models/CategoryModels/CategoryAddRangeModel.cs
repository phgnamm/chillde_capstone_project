using Microsoft.AspNetCore.Http;

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
