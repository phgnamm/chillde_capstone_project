using System.ComponentModel.DataAnnotations;
using Chillde.Repositories.Enums;

namespace Chillde.Services.Models.PackageModels
{
    public class PackageAddModel
    {
        public Guid? PackageId { get; set; }
        [Required(ErrorMessage = "Package's description is required.")]
        public PackageName Name { get; set; }
        [Required(ErrorMessage = "Package's description is required.")]
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? DeliveryTime { get; set; }
        public int? SketchRevision { get; set; }
        public float ResponseTime { get; set; }
        public int? MaxQuantity { get; set; }

    }
}

