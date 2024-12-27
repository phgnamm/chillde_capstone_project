using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ItemAttributeModels
{
    public class ItemAttributeAddModel
    {
        public required Guid ItemId { get; set; }
        public required List<Guid> AttributeIds { get; set; }

    }
}
