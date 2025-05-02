using Chillde.Repositories.Models.ServiceModels;
using Chillde.Services.Models.ResponseModels;


namespace Chillde.Services.Interfaces
{
    public interface IOpenAiService
    {
        Task<float[]> GetEmbeddingAsync(List<string> texts);
        //Task<ResponseModel> GetEventAsync(string sourceLanguage, string targerLanguage);
        Task<ResponseModel> GetStructuredDataAsync(List<string> attributes);
        //Task<ResponseModel> GetEventAsync(string sourceLanguage, string targerLanguage);
        ResponseModel GetEvent(string sourceLanguageCode);
        Task<List<ServiceModel>> RerankTopServicesWithGPTAsync(
            string eventDescription,
            List<ServiceModel> candidates,
            int topN = 10);
    }
}
