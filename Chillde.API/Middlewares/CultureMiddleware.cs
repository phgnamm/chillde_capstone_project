using Microsoft.AspNetCore.Localization;

namespace Chillde.API.Middlewares
{
    public class CultureMiddleware
    {
        private readonly RequestDelegate _next;

        public CultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            var culture = path.Split('/').Skip(1).FirstOrDefault();
            if (culture != null && (culture == "en" || culture == "vi"))
            {
                var requestCulture = new RequestCulture(culture);
                context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(requestCulture, null));
            }

            await _next(context);
        }
    }
}
