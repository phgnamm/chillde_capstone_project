using Chillde.Services.Models.PackageFeatureModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.PackageModels
{
    public class PackageAddAllModel : PackageAddModel
    {
        public Guid PackageId { get; set; }
        public Guid ServiceId { get; set; }
        public List<PackageFeatureAddModel> PackageFeatureModels { get; set; } = new List<PackageFeatureAddModel>();
    }
}
