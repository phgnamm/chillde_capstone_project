using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.TranslationModels;
using Microsoft.AspNetCore.Http;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System.Threading.Channels;

namespace Chillde.Services.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOpenAIService _openAIService;

        public TranslationService(
            IUnitOfWork unitOfWork,
            IOpenAIService openAIService)
        {
            _unitOfWork = unitOfWork;
            _openAIService = openAIService;
        }

        public async Task<ResponseModel> TranslateAsync(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                var chatRequest = new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem($"You are a translator from {sourceLanguageCode} to {targetLanguageCode}. Translate the following text, keeping any special formatting or technical terms unchanged."),
                        ChatMessage.FromUser(text)
                    },
                    Model = "gpt-3.5-turbo",
                    Temperature = 0.3f,
                    MaxTokens = 1000,
                    TopP = 1
                };

                var completionResult = await _openAIService.ChatCompletion.CreateCompletion(chatRequest);

                if (!completionResult.Successful)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Failed to translate text."
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = completionResult.Choices.FirstOrDefault()?.Message.Content.Trim() ?? string.Empty
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

        public async Task<TranslationResponseModel> TranslateMultipleFieldsAsync(Dictionary<string, string> fieldsToTranslate, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                if (fieldsToTranslate == null || fieldsToTranslate.Count == 0)
                {
                    return new TranslationResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "No fields provided for translation."
                    };
                }
                var combinedText = string.Join("\n", fieldsToTranslate.Select(f => $"[{f.Key}] {f.Value}"));

                var chatRequest = new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem(
                    $"You are a translator from {sourceLanguageCode} to {targetLanguageCode}. Translate the following fields separately, keeping the format and context intact."),
                ChatMessage.FromUser(combinedText)
            },
                    Model = "gpt-3.5-turbo",
                    Temperature = 0.3f,
                    MaxTokens = 1000,
                    TopP = 1
                };

                var completionResult = await _openAIService.ChatCompletion.CreateCompletion(chatRequest);

                if (!completionResult.Successful)
                {
                    return new TranslationResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Failed to translate fields."
                    };
                }
                var translatedText = completionResult.Choices.FirstOrDefault()?.Message.Content.Trim() ?? string.Empty;
                var translatedFields = new Dictionary<string, string>();
                var translatedParts = translatedText.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in translatedParts)
                {
                    var fieldParts = part.Split(new[] { "] " }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (fieldParts.Length == 2)
                    {
                        var fieldName = fieldParts[0].TrimStart('[');
                        translatedFields.Add(fieldName, fieldParts[1].Trim());
                    }
                }

                return new TranslationResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Fields translated successfully.",
                    TranslatedFields = translatedFields
                };
            }
            catch (Exception ex)
            {
                return new TranslationResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseModel> SaveTranslationAsync(TransaltionAddModel addModel, string targetLanguageCode)
        {
            try
            {
                var languageId = await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode);

                var translation = new Translation
                {
                    EntityType = addModel.EntityType,
                    EntityId = addModel.EntityId,
                    FieldName = addModel.FieldName,
                    TranslationText = addModel.TranslationText,
                    LanguageId = (Guid)languageId
                };

                await _unitOfWork.TranslationRepository.AddAsync(translation);
                var changes = await _unitOfWork.SaveChangeAsync();
                return changes > 0
                   ? new ResponseModel
                   {
                       Code = StatusCodes.Status200OK,
                       Message = "Translation added successfully."
                   }
                   : new ResponseModel
                   {
                       Code = StatusCodes.Status500InternalServerError,
                       Message = "Failed to save the translation."
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

        public async Task<ResponseModel> UpdateAsync(TransaltionAddModel model, LanguageCode languageCode)
        {
            if (model == null || string.IsNullOrEmpty(model.EntityType) || model.EntityId == Guid.Empty || string.IsNullOrEmpty(model.FieldName))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid data provided."
                };
            }

            try
            {
                var existingLanguage = await _unitOfWork.LanguageRepository.GetByCodeAsync(languageCode);
                var existingTranslation = await _unitOfWork.TranslationRepository.GetTranslationAsync(model.EntityType, model.EntityId, model.FieldName, existingLanguage!.Id);

                if (existingTranslation == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Translation not found."
                    };
                }

                existingTranslation.TranslationText = model.TranslationText;
                _unitOfWork.TranslationRepository.Update(existingTranslation);
                var changes = await _unitOfWork.SaveChangeAsync();

                return changes > 0
                     ? new ResponseModel
                     {
                         Code = StatusCodes.Status200OK,
                         Message = "Translation updated successfully."
                     }
                     : new ResponseModel
                     {
                         Code = StatusCodes.Status500InternalServerError,
                         Message = "Failed to update the translation."
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

        public async Task<ResponseModel> DeleteAsync(string entityType, Guid entityId, string fieldName, Guid languageId)
        {
            try
            {
                var existingTranslation = await _unitOfWork.TranslationRepository.GetTranslationAsync(entityType, entityId, fieldName, languageId);
                if (existingTranslation == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Translation not found."
                    };
                }

                _unitOfWork.TranslationRepository.HardRemove(existingTranslation);
                var changes = await _unitOfWork.SaveChangeAsync();

                return changes > 0
                      ? new ResponseModel
                      {
                          Code = StatusCodes.Status200OK,
                          Message = "Translation deleted successfully."
                      }
                      : new ResponseModel
                      {
                          Code = StatusCodes.Status500InternalServerError,
                          Message = "Failed to delete the translation."
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

        public async Task<ResponseModel> TranslateMultipleAsync(
            Dictionary<string, string> textsToTranslate,
            string entityType,
            Guid entityId,
            string sourceLanguageCode)
        {
            var targetLanguageCode = sourceLanguageCode.ToLower() == "vi" ? "en" : "vi";
            var translations = new Dictionary<string, string>();

            try
            {
                foreach (var item in textsToTranslate)
                {
                    var fieldName = item.Key;
                    var text = item.Value;

                    var translatedText = await TranslateAsync(text, sourceLanguageCode, targetLanguageCode);
                    translations.Add(fieldName, translatedText.Message);

                    if (sourceLanguageCode.ToLower() == "en")
                    {
                        await SaveTranslationAsync(new TransaltionAddModel
                        {
                            TranslationText = translatedText.Message,
                            EntityType = entityType,
                            EntityId = entityId,
                            FieldName = fieldName
                        }, targetLanguageCode);
                    }
                    else
                    {
                        await SaveTranslationAsync(new TransaltionAddModel
                        {
                            TranslationText = text,
                            EntityType = entityType,
                            EntityId = entityId,
                            FieldName = fieldName
                        }, sourceLanguageCode);
                    }
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Translations processed successfully."
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
