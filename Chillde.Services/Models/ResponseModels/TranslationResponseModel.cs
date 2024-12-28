
namespace Chillde.Services.Models.ResponseModels
{
    public class TranslationResponseModel : ResponseModel
    {
        public Dictionary<string, string> TranslatedFields { get; set; } = new Dictionary<string, string>();
    }
}
