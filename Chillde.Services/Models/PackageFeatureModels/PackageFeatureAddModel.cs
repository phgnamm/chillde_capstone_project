using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.PackageFeatureModels
{
    public class PackageFeatureAddModel
    {
        [Required]
        public Guid FeatureId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public bool IsExtra { get; set; }
        [Required]
        public decimal AdditionalCost { get; set; }
        [Required]
        public int AdditionalDay { get; set; }
        [Required]
        public bool IsChecked { get; set; }
        [Required]
        public int MaxQuantity { get; set; }
    }

    public class PackageFeatureAddModelForFeature
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public bool IsExtra { get; set; }
        [Required]
        public decimal AdditionalCost { get; set; }
        [Required]
        public int AdditionalDay { get; set; }
        [Required]
        public bool IsChecked { get; set; }
        [Required]
        public int MaxQuantity { get; set; }
    }
}
