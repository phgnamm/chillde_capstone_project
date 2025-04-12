using AutoMapper;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FAQModels;
using Chillde.Services.Helpers;
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
        private readonly IRedisHelper _redisHelper;
        private readonly IMapper _mapper;

        public FAQService(IUnitOfWork unitOfWork, IBadWordFilterService badWordFilterService, IRedisHelper redisHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _badWordFilterService = badWordFilterService;
            _redisHelper = redisHelper;
            _mapper = mapper;
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

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
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

                FAQModel faqModel = _mapper.Map<FAQModel>(faq);

                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _redisHelper.InvalidateCacheByPatternAsync($"faqs_{id}");
                    await _redisHelper.InvalidateCacheByPatternAsync("faqs_*");
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "FAQ successfully updated.",
                        Data = faqModel
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status204NoContent,
                        Message = "No changes detected."
                    };
                }
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
                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _redisHelper.InvalidateCacheByPatternAsync($"faqs_{id}");
                    await _redisHelper.InvalidateCacheByPatternAsync("faqs_*");
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "FAQ successfully deleted.",
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status204NoContent,
                        Message = "No changes detected."
                    };
                }
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
