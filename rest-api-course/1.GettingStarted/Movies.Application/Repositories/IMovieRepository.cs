using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public interface IMovieRepository
    {
        Task<Movie?> GetMovieByIdAsync(Guid id);

        Task<List<Movie>?> GetAllAsync();

        Task<bool> CreateMovieAsync(Movie movie);

        Task<bool> UpdateMovieAsync(Movie movie);

        Task<bool> DeleteByIdAsync(Guid id);
    }
}
