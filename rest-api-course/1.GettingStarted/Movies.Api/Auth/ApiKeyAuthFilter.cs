using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Movies.Api.Auth
{
    /// <summary>
    /// Authorization filter for validating API key from request headers.
    /// </summary>
    public class ApiKeyAuthFilter : IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthFilter(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if(!context.HttpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, 
                out var extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult("API Key Missing!");
                return;
            }

            var apiKey = _configuration["ApiKey"]!;
            if(!apiKey.Equals(extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult("Invalid API Key!");
                return;
            }
        }
    }
}
