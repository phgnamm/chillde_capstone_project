using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.PackageFeatureModels
{
    public class PackageFeatureAddModel
    {
        [Required]
        public string Question { get; set; }
        [Required]
        public bool IsInformationRequired { get; set; }
        [Required]
        public bool IsExtra { get; set; }
        [Required]
        public decimal AdditionalCost { get; set; }
        [Required]
        public int AdditionalDay { get; set; }
    }
}
