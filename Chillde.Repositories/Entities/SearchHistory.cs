using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class SearchHistory : BaseEntity
    {
        public string? SearchText { get; set; }

        // Relationship
        public Account CreatedBy { get; set; } = null!;

    }
}
