using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SuggestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IOpenAiService
    {
        Task<float[]> GetEmbeddingAsync(List<string> texts);
        Task<ResponseModel> GetRecommendationsAsync(SuggestAddModel suggestAddModel);
    }
}
