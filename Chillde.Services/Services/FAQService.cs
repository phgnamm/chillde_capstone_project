using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class FAQService : IFAQService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBadWordFilterService _badWordFilterService;

        public FAQService(IUnitOfWork unitOfWork, IBadWordFilterService badWordFilterService)
        {
            _unitOfWork = unitOfWork;
            _badWordFilterService = badWordFilterService;
        }

        public async Task<ResponseModel> UpdateAsync(FAQAddAndUpdateModel faqAddAndUpdateModel, Guid id, string sourceLanguageCode)
        {
            try
            {
                string[] fieldsToCheck = { faqAddAndUpdateModel.Question, faqAddAndUpdateModel.Answer };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code != StatusCodes.Status200OK)
                        return response;
                }
                var faq = await _unitOfWork.FAQRepository.GetAsync(id);
                if (faq == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "FAQ not found."
                    };
                }

                faq.Question = faqAddAndUpdateModel.Question;
                faq.Answer = faqAddAndUpdateModel.Answer;

                _unitOfWork.FAQRepository.Update(faq);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "FAQ successfully updated.",
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseModel> HardDeleteAsync(Guid id)
        {
            try
            {
                var faq = await _unitOfWork.FAQRepository.GetAsync(id);
                if (faq == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "FAQ not found."
                    };
                }

                _unitOfWork.FAQRepository.HardRemove(faq);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "FAQ successfully deleted.",
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                };
            }
        }
    }
}
