using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.TranslationModels;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;

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

        public async Task<string> TranslateAsync(string text, string sourceLanguageCode, string targetLanguageCode)
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
                throw new Exception("Failed to translate text using OpenAI");
            }

            return completionResult.Choices.FirstOrDefault()?.Message.Content.Trim() ?? string.Empty;
        }

        public async Task SaveTranslationAsync(TransaltionAddModel addModel, string targetLanguageCode)
        {
            var languageId = await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode);

            var translation = new Translation
            {
                EntityType = addModel.EntityType,
                EntityId = addModel.EntityId,
                FieldName = addModel.FieldName,
                TranslationText = addModel.Text,
                LanguageId = (Guid)languageId
            };

            await _unitOfWork.TranslationRepository.AddAsync(translation);
        }

        public async Task<Dictionary<string, string>> TranslateAndSaveMultipleAsync(
            Dictionary<string, string> textsToTranslate,
            string entityType,
            Guid entityId,
            string sourceLanguageCode)
        {
            var targetLanguageCode = sourceLanguageCode.ToLower() == "vi" ? "en" : "vi";
            var translations = new Dictionary<string, string>();

            foreach (var item in textsToTranslate)
            {
                var fieldName = item.Key;
                var text = item.Value;

                var translatedText = await TranslateAsync(text, sourceLanguageCode, targetLanguageCode);
                translations.Add(fieldName, translatedText);

                if (sourceLanguageCode.ToLower() == "en")
                {
                    await SaveTranslationAsync(new TransaltionAddModel
                    {
                        Text = translatedText,
                        EntityType = entityType,
                        EntityId = entityId,
                        FieldName = fieldName
                    }, targetLanguageCode);
                }
                else
                {
                    await SaveTranslationAsync(new TransaltionAddModel
                    {
                        Text = text,
                        EntityType = entityType,
                        EntityId = entityId,
                        FieldName = fieldName
                    }, sourceLanguageCode);
                }
            }

            return translations;
        }
    }
}
