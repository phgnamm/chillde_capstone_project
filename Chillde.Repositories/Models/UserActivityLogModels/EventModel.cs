using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.UserActivityLogModels
{
    public class EventModel
    {
        public string NameVi { get; set; } = string.Empty; 
        public string NameEn { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public bool IsLunar { get; set; }
    }
}
