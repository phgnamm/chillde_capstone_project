using System.ComponentModel.DataAnnotations;
using Chillde.Repositories.Enums;

namespace Chillde.Services.Models.PackageModels
{
    public class PackageAddModel
    {
        [Required(ErrorMessage = "Package's name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Package's description is required.")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Package's price must be greater than or equal 1 and lower than or equal 10,000 USD.")]
        [Range(1, 10000, ErrorMessage = "Package's price must be greater than or equal 1 and lower than or equal 10,000 USD.")]
        public decimal Price { get; set; }
        [Required]
        public DateTime? DeliveryTime { get; set; }
        [Required]
        public int? SketchRevision { get; set; }
        [Required]
        public float? DeliveryRevision { get; set; }
        
    }
}

