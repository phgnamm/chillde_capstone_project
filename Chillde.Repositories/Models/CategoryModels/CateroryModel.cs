using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.CategoryModels
{
    public class CateroryModel : BaseEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
    }
}
