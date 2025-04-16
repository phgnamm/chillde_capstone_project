using Chillde.Services.Models.PackageFeatureModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.FeatureModels
{
    public class FeatureOfferUpdateModel
    {
            public Guid? FeatureId { get; set; }
            [Required]
            public required List<PackageFeatureAddModelForFeature> PackageFeatureAddModels { get; set; }
    }
}
