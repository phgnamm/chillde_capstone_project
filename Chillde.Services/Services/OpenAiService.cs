using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Chillde.Services.Models.OpenAIModels;
using Chillde.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Chillde.Services.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly IConfiguration _configuration;

        public OpenAiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<float[]> GetEmbeddingAsync(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Input cannot be empty.", nameof(description));

            // Lấy API key từ cấu hình
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API key is missing.");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "text-embedding-ada-002",
                input = description
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
