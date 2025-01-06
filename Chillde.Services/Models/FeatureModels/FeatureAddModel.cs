using Chillde.Services.Models.PackageFeatureModels;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.FeatureModels
{
    public class FeatureAddModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public List<PackageFeatureAddModel> PackageFeatures { get; set; }   
    }
}
