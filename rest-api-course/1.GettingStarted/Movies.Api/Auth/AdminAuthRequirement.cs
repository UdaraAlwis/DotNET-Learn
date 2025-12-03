using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Movies.Api.Auth
{
    /// <summary>
    /// Represents an authorization requirement that checks for administrative access.
    /// </summary>
    /// <remarks>This requirement verifies if a user has administrative privileges by checking for a specific
    /// claim or by validating an API key in the request headers. If the user is identified as an admin, the
    /// authorization context is marked as successful.</remarks>
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
            // Add the user identity into the claims so that we know this is an admin user
            identity.AddClaim(new Claim("userid", "61325d03-5f97-4d46-8ce0-e613a187a94b"));
            context.Succeed(this);
            return Task.CompletedTask;
        }
    }
}
