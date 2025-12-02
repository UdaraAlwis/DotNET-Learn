using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Movies.Api.Auth
{
    public class AdminAuthRequirement : IAuthorizationHandler, IAuthorizationRequirement
    {
        private readonly string _apiKey;

        public AdminAuthRequirement(string apiKey)
        {
            _apiKey = apiKey;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if(context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
            {
                context.Succeed(this);
                return Task.CompletedTask;
            }

            var httpContext = context.Resource as HttpContext;
            if(httpContext is null)
            {
                return Task.CompletedTask;
            }

            if (!httpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName,
                out var extractedApiKey))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (_apiKey != extractedApiKey)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var identity = (ClaimsIdentity)httpContext.User.Identity!;
            identity.AddClaim(new Claim("userid", "61325d03-5f97-4d46-8ce0-e613a187a94b"));
            context.Succeed(this);
            return Task.CompletedTask;
        }
    }
}
