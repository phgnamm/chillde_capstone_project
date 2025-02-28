using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IRequestAttributeService
    {
        Task<ResponseModel> RemoveAttributeValue(Guid id);
        Task<ResponseModel> RemoveAttribute(Guid id);

    }
}
