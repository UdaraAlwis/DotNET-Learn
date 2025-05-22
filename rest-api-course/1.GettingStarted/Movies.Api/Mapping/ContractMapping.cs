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

        public static MovieResponse ToMovieResponse(this Movie movie)
        {
            return new MovieResponse
            {
                Id = movie.Id,
                Title = movie.Title,
                YearOfRelease = movie.YearOfRelease,
                Genres = movie.Genres
            };
        }

        public static MoviesResponse ToMoviesResponse(this IEnumerable<Movie> movies)
        {
            return new MoviesResponse { Items = movies.Select(ToMovieResponse) };
        }
    }
}
