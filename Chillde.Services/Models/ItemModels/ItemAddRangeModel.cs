using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ItemModels
{
    public class ItemAddRangeModel
    {
        public required List<ItemAddRequestModel> ItemAddRequestModels { get; set; }
        public List<IFormFile>? ImageUrls { get; set; }
    }
    public class ItemAddRequestModel
    {
        public required string Name { get; set; }
        public string? Code { get; set; }
    }
}
