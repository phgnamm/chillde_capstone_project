using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.BadWordFilterModels
{
    public class BadWordFilterModel
    {
        public string content {  get; set; }
        public string catalog { get; set; } = "strict";
        public string censorCharacter { get; set; } = "";
    }
}
