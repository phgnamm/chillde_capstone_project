using Chillde.Repositories.Entities;
using Chillde.Services.Models.LanguageModels;
using Chillde.Services.Models.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface ILanguageService
    {
        Task<ResponseModel> GetAsync(Guid id);
        Task<ResponseModel> GetAllAsync();
        Task<ResponseModel> AddAsync(LanguageAddModel model);
        Task<ResponseModel> Update(Guid id, LanguageAddModel model);
        Task<ResponseModel> DeleteAsync(Guid id);
    }
}
