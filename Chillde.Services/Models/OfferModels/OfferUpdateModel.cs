using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.OfferModels
{
    public class OfferUpdateModel : OfferAddModel
    {
        public OfferStatus? Status { get; set; }
    }
}
