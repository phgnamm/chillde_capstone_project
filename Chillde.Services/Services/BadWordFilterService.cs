using Chillde.Repositories.Models.BadWordFilterModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.BadWordFilterModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Chillde.Services.Services
{
    public class BadWordFilterService : IBadWordFilterService
    {
        private readonly string _url;
        private readonly string _userId;
        private readonly string _apiKey;

        private readonly HttpClient _httpClient;
        private readonly ITranslationService _translationService;

        public BadWordFilterService(HttpClient httpClient, ITranslationService translationService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _translationService = translationService;
            _url = configuration["NeutrinoApi:Url"];
            _userId = configuration["NeutrinoApi:UserID"];
            _apiKey = configuration["NeutrinoApi:ApiKey"];
        }

        public async Task<ResponseModel> FilterBadWordsAsync(BadWordFilterModel badWordFilterModel, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                string? translatedMessage = null;

                if (!string.IsNullOrEmpty(sourceLanguageCode) && !string.IsNullOrEmpty(targetLanguageCode))
                {
                    var translationResponse = await _translationService.TranslateAsync(badWordFilterModel.Content, sourceLanguageCode, targetLanguageCode);

                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = "Failed to translate message."
                        };
                    }

                    translatedMessage = translationResponse.Message;
                    badWordFilterModel.Content = targetLanguageCode == "en" ? translatedMessage : badWordFilterModel.Content;
                }

                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("content", badWordFilterModel.Content),
                    new KeyValuePair<string, string>("catalog", badWordFilterModel.Catalog),
                    new KeyValuePair<string, string>("censor-character", badWordFilterModel.CensorCharacter)
                });

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-ID", _userId);
                _httpClient.DefaultRequestHeaders.Add("API-Key", _apiKey);

                var response = await _httpClient.PostAsync(_url, formData);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<BadWordFilterResponse>(responseContent);

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Feedback created successfully.",
                    Data = result
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
