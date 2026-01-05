using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies
{
    public static class UpdateMovieEndpoint
    {
        public const string Name = "UpdateMovie";

        public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
        {
            app.MapPut(ApiEndpoints.Movies.Update, async (
                Guid id, UpdateMovieRequest request, 
                IMovieService movieService, IOutputCacheStore outputCacheStore, HttpContext context, CancellationToken cancellationToken) =>
            {
                var movieToUpdate = request.ToMovie(id);
                var userId = context.GetUserId();
                var updatedMovie = await movieService.UpdateAsync(movieToUpdate, userId, cancellationToken);
                if (updatedMovie == null)
                    return Results.NotFound();

                await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
                var response = movieToUpdate.MapToResponse();
                return TypedResults.Ok(response);
            }).WithName(Name)
            .Produces<MoviesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization(AuthConstants.TrustedMemberPolicyName);

            return app;
        }
    }
}
