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
        Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken);

        Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

        Task<List<Movie>?> GetAllAsync(CancellationToken cancellationToken);

        Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken);

        Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
