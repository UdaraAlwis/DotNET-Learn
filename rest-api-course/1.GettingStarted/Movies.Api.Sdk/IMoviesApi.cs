using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Refit;

namespace Movies.Api.Sdk
{
    [Headers("Authorization: Bearer")]
    public interface IMoviesApi
    {
        [Get(ApiEndpoints.Movies.Get)]
        Task<MovieResponse> GetMovieAsync(string idOrSlug);

        [Get(ApiEndpoints.Movies.GetAll)]
        Task<MoviesResponse> GetMoviesAsync(GetAllMoviesRequest getAllMoviesRequest);

        [Post(ApiEndpoints.Movies.Create)]
        Task<MovieResponse> CreateMovieAsync(CreateMovieRequest createMovieRequest);

        [Put(ApiEndpoints.Movies.Update)]
        Task<MovieResponse> UpdateMovieAsync(Guid id, UpdateMovieRequest updateMovieRequest);

        [Delete(ApiEndpoints.Movies.Delete)]
        Task DeleteMovieAsync(Guid id);

        [Put(ApiEndpoints.Movies.Rate)]
        Task RateMovieAsync(Guid id, RateMovieRequest rateMovieRequest);

        [Delete(ApiEndpoints.Movies.DeleteRating)]
        Task DeleteMovieRatingAsync(Guid id);

        [Get(ApiEndpoints.Ratings.GetUserRatings)]
        Task<IEnumerable<MovieRatingResponse>> GetUserRatingsAsync();
    }
}
