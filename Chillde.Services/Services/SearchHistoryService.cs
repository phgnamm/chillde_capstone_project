using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class SearchHistoryService : ISearchHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchHistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> Delete(Guid id)
        {
            var searchHistory = await _unitOfWork.SearchHistoryRepository.GetAsync(id);
            if (searchHistory == null) {
                return new ResponseModel { Message = "Cannot found.", Code = StatusCodes.Status404NotFound };
            }
             _unitOfWork.SearchHistoryRepository.HardRemove(searchHistory);
            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0 ? new ResponseModel { Message = "Delete successfully." } : new ResponseModel { Message = "Delete unsuccessfully", Code = StatusCodes.Status400BadRequest };
        }
    }
}
