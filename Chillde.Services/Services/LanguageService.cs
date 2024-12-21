using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.LanguageModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LanguageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseModel> GetAsync(Guid id)
        {
            try
            {
                var language = await _unitOfWork.LanguageRepository.GetAsync(id);
                if (language == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Language not found"
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = language
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while retrieving the language: {ex.Message}"
                };
            }
        }
        public async Task<ResponseModel> GetAllAsync()
        {
            try
            {
                var languages = await _unitOfWork.LanguageRepository.GetAllAsync();
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = languages.Data
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while retrieving all languages: {ex.Message}"
                };
            }
        }
        public async Task<ResponseModel> AddAsync(LanguageAddModel model)
        {
            try
            {
                var language = new Language
                {
                    Code = model.Code,
                    Name = model.Name
                };

                await _unitOfWork.LanguageRepository.AddAsync(language);
                var saveResult = await _unitOfWork.SaveChangeAsync();
                if (saveResult > 0)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status201Created,
                        Message = "Language successfully created",
                        Data = language
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "An error occurred while saving the language."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while creating the language: {ex.Message}"
                };
            }
        }
        public async Task<ResponseModel> Update(Guid id, LanguageAddModel model)
        {
            try
            {
                var language = await _unitOfWork.LanguageRepository.GetAsync(id);

                if (language == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Language not found"
                    };
                }

                language.Name = model.Name;
                language.Code = model.Code;
                _unitOfWork.LanguageRepository.Update(language);
                var saveResult = await _unitOfWork.SaveChangeAsync();
                if (saveResult > 0)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Language successfully updated",
                        Data = language
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "An error occurred while saving the updated language."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while updating the language: {ex.Message}"
                };
            }
        }
        public async Task<ResponseModel> DeleteAsync(Guid id)
        {
            try
            {
                var language = await _unitOfWork.LanguageRepository.GetAsync(id);

                if (language == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Language not found"
                    };
                }

                _unitOfWork.LanguageRepository.SoftRemove(language);
                var saveResult = await _unitOfWork.SaveChangeAsync();
                if (saveResult > 0)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Language successfully deleted"
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "An error occurred while deleting the language."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while deleting the language: {ex.Message}"
                };
            }
        }
    }
}
