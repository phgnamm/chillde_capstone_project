using Chillde.Repositories.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.PackageModels
{
    public class PackageUpdateModel
    {
        public PackageName Name { get; set; }
        [Required(ErrorMessage = "Package's description is required.")]
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? DeliveryTime { get; set; }
        public int? SketchRevision { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int? MaxQuantity { get; set; }
    }
}
