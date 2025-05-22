using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly List<Movie> _movies = [];

        public Task<bool> CreateMovieAsync(Movie movie)
        {
            _movies.Add(movie);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteByIdAsync(Guid id)
        {
            var removedCount = _movies.RemoveAll(x => x.Id == id);
            if (removedCount > 0)
                return Task.FromResult(true);
            else
                return Task.FromResult(false);
        }

        public Task<List<Movie>?> GetAllAsync(Guid id)
        {
            return Task.FromResult(_movies ?? null);
        }

        public Task<Movie?> GetMovieByIdAsync(Guid id)
        {
            return Task.FromResult(_movies.FirstOrDefault(x => x.Id == id));
        }

        public Task<bool> UpdateMovieAsync(Movie movie)
        {
            var movieIndex = _movies.FindIndex(x => x.Id == movie.Id);
            if(movieIndex != -1)
            {
                _movies[movieIndex] = movie;
                return Task.FromResult(true);
            }
            else
                return Task.FromResult(false);
        }
    }
}
