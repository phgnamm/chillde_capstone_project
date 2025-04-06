using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.AccountModels
{
    public class BanAccountRoleModel
    {
        public Guid AccountId { get; set; }
        public Role Role { get; set; }
    }
}
