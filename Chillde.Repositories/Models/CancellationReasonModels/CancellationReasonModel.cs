using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.CancellationReasonModels
{
    public class CancellationReasonModel : BaseEntity
    {
       
        public string Name { get; set; } = null!;

        public float Value { get; set; }

        public Chillde.Repositories.Enums.Role RoleType { get; set; }
    }
}
