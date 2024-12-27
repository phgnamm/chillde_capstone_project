using Chillde.Services.Models.AttributeModels;
using Chillde.Services.Models.ItemAttributeModels;
using Chillde.Services.Models.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IAttributeService
    {
        Task<ResponseModel> Add(AttributeAddModel attributeAddModel);
        Task<ResponseModel> GetAll(AttributeFilterModel attributeFilterModel);
        Task<ResponseModel> GetAttributeValueById(Guid id);
    }
}
