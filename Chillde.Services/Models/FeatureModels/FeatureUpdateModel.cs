using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.FeatureModels
{
    public class FeatureUpdateModel
    {
        [Required]
        public string Name { get; set; }
    }
}
