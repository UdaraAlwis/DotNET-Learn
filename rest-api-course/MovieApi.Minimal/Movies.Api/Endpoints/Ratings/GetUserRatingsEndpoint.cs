using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Ratings
{
    public static class GetUserRatingsEndpoint
    {
        public const string Name = "GetUserRatings";

        public static IEndpointRouteBuilder MapGetUserRatings(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.Ratings.GetUserRatings, static async (IRatingService ratingService, HttpContext context, 
                CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var ratings = await ratingService.GetUserRatingsAsync(userId!.Value, cancellationToken);
                var response = ratings.MapToResponse();

                return TypedResults.Ok(response);
            }).WithName(Name)
            .RequireAuthorization();

            return app;
        }
    }
}
