using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.LanguageModels
{
    public class LanguageModel
    {
        public LanguageCode Code { get; set; }
        public string? Name { get; set; }
    }
}
