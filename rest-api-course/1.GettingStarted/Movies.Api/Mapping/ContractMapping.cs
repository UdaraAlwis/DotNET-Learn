using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping
{
    public static class ContractMapping
    {
        public static Movie ToMovie(this CreateMovieRequest createMovieRequest)
        {
            return new Movie
            {
                Id = Guid.NewGuid(),
                Title = createMovieRequest.Title,
                YearOfRelease = createMovieRequest.YearOfRelease,
                Genres = createMovieRequest.Genres.ToList(),
            };
        }

        public static Movie ToMovie(this UpdateMovieRequest updateMovieRequest, Guid id)
        {
            return new Movie 
            { 
                Id = id,
                Title = updateMovieRequest.Title,
                YearOfRelease = updateMovieRequest.YearOfRelease,
                Genres = updateMovieRequest.Genres.ToList(),
            };
        }

        public static MovieResponse MapToResponse(this Movie movie)
        {
            return new MovieResponse
            {
                Id = movie.Id,
                Title = movie.Title,
                YearOfRelease = movie.YearOfRelease,
                Rating = movie.Rating,
                UserRating = movie.UserRating,
                Slug = movie.Slug,
                Genres = movie.Genres
            };
        }

        public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies)
        {
            return new MoviesResponse { Items = movies.Select(MapToResponse) };
        }

        public static IEnumerable<MovieRatingResponse> MapToResponse(this IEnumerable<MovieRating> ratings)
        {
            return ratings.Select(rating => new MovieRatingResponse
            { 
                MovieId = rating.MovieId,
                Slug = rating.Slug,
                Rating = rating.Rating
            });
        }

        public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
        {
            return new GetAllMoviesOptions
            {
                Title = request.Title,
                YearOfRelease = request.YearOfRelease,
                SortField = request.SortBy?.Trim('+','-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted : 
                    request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending
            };
        }

        public static GetAllMoviesOptions WithUser(this GetAllMoviesOptions options, Guid? userId)
        {
            options.UserId = userId;
            return options;
        }
    }
}
