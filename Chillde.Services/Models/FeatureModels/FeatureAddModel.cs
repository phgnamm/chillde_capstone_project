using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.FeatureModels
{
    public class FeatureAddModel
    {
        [Required(ErrorMessage ="Feature's name is required!")]
        public string Name { get; set; }
        public string? Question { get; set; }
        public bool? IsInformationRequired { get; set; }
        public bool? IsExtra { get; set; }
        public decimal? AdditionalCost { get; set; }
        public int? AdditionalDay { get; set; }
    }
}
