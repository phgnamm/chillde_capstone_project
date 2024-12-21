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
        Task<string> TranslateAsync(string text, string sourceLanguageCode, string targetLanguageCode);
        Task SaveTranslationAsync(TransaltionAddModel addModel, string targetLanguageCode);
        Task<Dictionary<string, string>> TranslateAndSaveMultipleAsync(
            Dictionary<string, string> textsToTranslate,
            string entityType,
            Guid entityId,
            string sourceLanguageCode);
    }
}
