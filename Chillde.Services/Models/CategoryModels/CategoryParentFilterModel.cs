using Chillde.Services.Common;

namespace Chillde.Services.Models.CategoryModels;

    public class CategoryParentFilterModel : FilterParameter
    {
    public required Guid ParentId { get; set; } 
    }