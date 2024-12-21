using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.TranslationModels
{
    public class TransaltionAddModel
    {
        public string Text { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public Guid EntityId { get; set; }
        public string FieldName { get; set; } = null!;
    }
}
