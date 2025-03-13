namespace Chillde.API.Helpers
{
    public static class LanguageHelper
    {
        public static string GetTargetLanguageCode(string sourceLanguageCode)
        {
            var primaryLanguage = sourceLanguageCode.Split(',').FirstOrDefault();
            var languageCode = primaryLanguage?.Split('-').FirstOrDefault()?.ToLower();

            return languageCode switch
            {
                "vi" => "en",
                "en" => "vi", 
                _ => "en"   
            };
        }
        public static string GetSourceLanguageCode(string acceptLanguage)
        {
            if (string.IsNullOrWhiteSpace(acceptLanguage))
            {
                return "en"; 
            }

            var primaryLanguage = acceptLanguage.Split(',').FirstOrDefault();
            return primaryLanguage?.Split('-').FirstOrDefault()?.ToLower() ?? "en"; 
        }
    }

}
