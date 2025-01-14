using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Chillde.Repositories.Common;
using Chillde.Services.Models.SuggestModels;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Chillde.Services.Models.OpenAIModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.Extensions.Configuration;

namespace Chillde.Services.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly IConfiguration _configuration;
        private readonly IOpenAIService _openAiService;
        private readonly ModelConfigurationOptions _modelConfigurationOptions;

        public OpenAiService(IConfiguration configuration, IOpenAIService openAiService,
            IOptions<ModelConfigurationOptions> modelConfigurationOptions
)
        {
            _openAiService = openAiService;
            _modelConfigurationOptions = modelConfigurationOptions.Value;
            _configuration = configuration;
        }

        public async Task<ResponseModel> GetRecommendationsAsync(SuggestAddModel suggestAddModel)
        {
            var fineTunedModel = _modelConfigurationOptions.FineTunedModelId;
            var defaultModel = _modelConfigurationOptions.DefaultModel;
            var eventPrompt = "What are the upcoming events in Vietnam within the next 1.5 months?";

            var eventResponse = await _openAiService.Completions.CreateCompletion(
                new CompletionCreateRequest
                {
                    Prompt = eventPrompt,
                    Model = defaultModel,
                    MaxTokens = 100
                });

            string? eventInfo = eventResponse?.Choices?.FirstOrDefault()?.Text?.Trim();
            if (string.IsNullOrEmpty(eventInfo))
            {
                eventInfo = "No upcoming events found.";
            }
            ResponseModel response;
            if (string.IsNullOrWhiteSpace(suggestAddModel.UserInput))
            {
                var eventRecommendation = await SuggestServicesBasedOnEvent(eventInfo, fineTunedModel);
                response = new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = eventRecommendation
                };
            }
            else
            {
                var userInputRecommendation = await SuggestServicesBasedOnUserInput(suggestAddModel.UserInput, fineTunedModel);
                response = new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = userInputRecommendation
                };
            }

            return response;
        }

        private async Task<string> SuggestServicesBasedOnEvent(string eventInfo, string fineTunedModel)
        {
            var prompt = $"The nearest event is: '{eventInfo}'. Suggest services related to it.";

            var recommendationResponse = await _openAiService.Completions.CreateCompletion(
                new CompletionCreateRequest
                {
                    Prompt = prompt,
                    Model = fineTunedModel,
                    MaxTokens = 150
                });

            return recommendationResponse?.Choices?.FirstOrDefault()?.Text?.Trim() ?? "No service recommendations available.";
        }

        private async Task<string> SuggestServicesBasedOnUserInput(string userInput, string fineTunedModel)
        {
            var prompt = $"Based on the user's input: '{userInput}', suggest suitable services.";

            var recommendationResponse = await _openAiService.Completions.CreateCompletion(
                new CompletionCreateRequest
                {
                    Prompt = prompt,
                    Model = fineTunedModel,
                    MaxTokens = 150
                });

            return recommendationResponse?.Choices?.FirstOrDefault()?.Text?.Trim() ?? "No suitable services found.";
        }
        public async Task<float[]> GetEmbeddingAsync(List<string> texts)
        {
            if (texts == null || texts.Count == 0)
            {
                throw new ArgumentException("Input cannot be empty.", nameof(texts));
            }

            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API key is missing.");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "text-embedding-ada-002",
                input = texts
            };

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/embeddings", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve embeddings: {response.StatusCode} - {errorContent}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<OpenAiEmbeddingResponse>();
            return responseData?.Data.FirstOrDefault()?.Embedding ?? throw new Exception("No embedding data returned.");
        }
        public class OpenAiEmbeddingResponse
        {
            public List<EmbeddingData> Data { get; set; } = new List<EmbeddingData>();
        }

        public class EmbeddingData
        {
            public float[] Embedding { get; set; } = Array.Empty<float>();
        }
    }
}
