using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.OfferModels
{
    public class OfferLocalierModel
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public Guid RequestId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid? CreatedById { get; set; }
        public DateTime CreationDate { get; set; }
    }

}
