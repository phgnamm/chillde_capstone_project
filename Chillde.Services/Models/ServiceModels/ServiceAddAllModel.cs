using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceAddAllModel : ServiceAddModel
    {
        public List<PackageAddModel> PackageAddAllModels { get; set; } = new List<PackageAddModel>();
        public List<FeatureAddModel> FeatureAddModels { get; set; } = new List<FeatureAddModel>();
    }
}
