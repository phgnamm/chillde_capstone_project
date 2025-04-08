using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.PackageFeatureModels
{
    public class PackageFeatureUpdateModel
    {
        public Guid? PackageId { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal? AdditionalCost { get; set; }
        public int? AdditionalDay { get; set; }
        public bool? IsExtra { get; set; }
        public bool? IsChecked { get; set; }
        public int MaxQuantity { get; set; }
    }
}
