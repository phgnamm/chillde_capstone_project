using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.ServiceModels
{
    public class ServiceModel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Similarity { get; set; }
        public string? AttachmentAlt { get; set; }
        public string? AttachmentUrl { get; set; }

    }
 
}
