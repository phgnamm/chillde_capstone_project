using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.SearchModels
{
    public class SearchModel : BaseEntity
    {
        public required string SearchText { get; set; }
    }
}
