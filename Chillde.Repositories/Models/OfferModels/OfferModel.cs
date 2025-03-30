using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.AccountModels;
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
        public string? Status { get; set; }
        public float? MinWeight { get; set; }
        public float? MaxWeight { get; set; }
        public Guid? RequestId { get; set; }
        public Guid? ServiceId { get; set; }
        public ICollection<OfferAttachment> OfferAttachments { get; set; } = new List<OfferAttachment>();
        public required AccountLiteModel CreatedBy { get; set; }
    }
}
