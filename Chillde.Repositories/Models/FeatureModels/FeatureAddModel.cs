using System.ComponentModel.DataAnnotations;

namespace Chillde.Repositories.Models.FeatureModels
{
    public class FeatureAddModel
    {
        [Required]
        public string Name { get; set; }
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
