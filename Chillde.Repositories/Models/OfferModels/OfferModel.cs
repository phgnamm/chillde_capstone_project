using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.OfferModels
{
    public class OfferModel : BaseEntity
    {
        public string? Message { get; set; }
        public OfferStatus Status { get; set; }
        public Guid RequestId { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ArtistName { get; set; }
    }
}
