using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.PackageModels
{
    public class PackageUpdateModel
    {
        [Required(ErrorMessage = "Package's price must be between 1 and 10,000 USD.")]
        [Range(1, 10000, ErrorMessage = "Package's price must be between 1 and 10,000 USD.")]
        public decimal Price { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
