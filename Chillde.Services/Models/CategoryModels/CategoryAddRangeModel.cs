using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Models.CategoryModels
{
    public class CategoryRangeModel
    {
        public required string Name { get; set; }
        public IFormFile? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }
    }

    public class CategoryAddRangeModel
    {
        public List<CategoryRangeModel>? CategoryAddRequestModels { get; set; }
        public Guid? ParentId { get; set; }
    }
}
