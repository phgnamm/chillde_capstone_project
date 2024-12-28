using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.BadWordFilterModels
{
    public class BadWordFilterResponse
    {
        [JsonProperty("censored-content")]
        public string CensoredContent { get; set; }

        [JsonProperty("is-bad")]
        public bool IsBad { get; set; }

        [JsonProperty("bad-words-list")]
        public List<string> BadWordsList { get; set; }

        [JsonProperty("bad-words-total")]
        public int BadWordsTotal { get; set; }
    }

}
