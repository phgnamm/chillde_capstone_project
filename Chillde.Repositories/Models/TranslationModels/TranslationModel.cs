using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.TranslationModels
{
    public class TranslationModel
    {
        public string EntityType { get; set; } = null!;
        public Guid EntityId { get; set; }
        public string FieldName { get; set; } = null!;
        public string TranslationText { get; set; } = null!;
    }
}
