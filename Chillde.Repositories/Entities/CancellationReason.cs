using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class CancellationReason : BaseEntity
    {
        public string Name { get; set; } = null!; 
        public int Value { get; set; }
    }
}
