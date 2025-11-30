using Movies.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Services
{
    public interface IMovieService
    {
        Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);

        Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default);

        Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default);

        Task<List<Movie>?> GetAllAsync(GetAllMoviesOptions getAllMoviesOptions, CancellationToken cancellationToken = default);

        Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken cancellationToken = default);

        Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default);
    }
}
