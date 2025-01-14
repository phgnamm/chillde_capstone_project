using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.ServiceAttachmentModels;

namespace Chillde.Repositories.Models.ServiceModels
{
    public class ServiceModel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Similarity { get; set; }
        public bool IsOffter { get; set; }
        public ServiceStatus Status { get; set; }
        public List<ServiceAttachment>? ServiceAttachments { get; set; }
    }
 
}
