using Chillde.Repositories.Enums;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.TranslationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface ITranslationService
    {
        Task<ResponseModel> TranslateAsync(string text, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> SaveTranslationAsync(TransaltionAddModel addModel, string targetLanguageCode);
        Task<ResponseModel> TranslateMultipleAsync(
            Dictionary<string, string> textsToTranslate,
            string entityType,
            Guid entityId,
            string sourceLanguageCode);
        Task<TranslationResponseModel> TranslateMultipleFieldsAsync(Dictionary<string, string> fieldsToTranslate, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> UpdateAsync(TransaltionAddModel model, LanguageCode languageCode);
        Task<ResponseModel> DeleteAsync(string entityType, Guid entityId, string fieldName, Guid languageId);
    }
}
