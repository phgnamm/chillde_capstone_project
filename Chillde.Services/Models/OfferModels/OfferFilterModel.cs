using Chillde.Repositories.Enums;
using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.OfferModels
{
    public class OfferFilterModel : FilterParameter
    {
        public Guid? ServiceId { get; set; }
        public Guid? CreatedById { get; set; }
        public OfferStatus? Status { get; set; }
    }
}
