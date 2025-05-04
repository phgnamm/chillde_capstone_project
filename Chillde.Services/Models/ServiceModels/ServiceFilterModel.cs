using Chillde.Repositories.Enums;
using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceFilterModel : FilterParameter
    {
        public bool IsAccountSuggestion { get; set; } = false;
        public bool IsEvent { get; set; } = false;
        public string? IdOrUserName { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? SubCategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinRate { get; set; }
        public double? MaxRate { get; set; }
        public int? MinDate { get; set; }
        public int? MaxDate { get; set; }
        public ServiceStatus? Status { get; set; }
    }
}
