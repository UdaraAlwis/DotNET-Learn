using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies
{
    public static class CreateMovieEndpoint
    {
        public const string Name = "CreateMovie";
        
        public static IEndpointRouteBuilder MapCreateMovie(this IEndpointRouteBuilder app)
        {
            app.MapPost(ApiEndpoints.Movies.Create, async (CreateMovieRequest request,
                IMovieService movieService, IOutputCacheStore outputCacheStore, CancellationToken cancellationToken) =>
            {
                var movieToCreate = request.ToMovie();
                await movieService.CreateAsync(movieToCreate, cancellationToken);
                await outputCacheStore.EvictByTagAsync("movies", cancellationToken);

                var response = movieToCreate.MapToResponse();

                return TypedResults.CreatedAtRoute(response, GetMovieEndpoint.Name, new { idOrSlug = movieToCreate.Id });
            }).WithName(Name)
            .Produces<MovieResponse>(StatusCodes.Status201Created)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization(AuthConstants.TrustedMemberPolicyName);

            return app;
        }
    }
}
