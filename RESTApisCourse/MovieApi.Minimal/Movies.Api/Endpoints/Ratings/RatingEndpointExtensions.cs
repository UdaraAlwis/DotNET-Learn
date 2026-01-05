namespace Movies.Api.Endpoints.Ratings
{
    public static class RatingEndpointExtensions
    {
        public static IEndpointRouteBuilder MapRoutingEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapRateMovie();
            app.MapDeleteRating();
            app.MapGetUserRatings();
            return app;
        }
    }
}
