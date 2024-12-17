using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class Language : BaseEntity
    {
        public LanguageCode Code { get; set; }
        public string? Name { get; set; }

        // Relationship
        public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();
    }
}
