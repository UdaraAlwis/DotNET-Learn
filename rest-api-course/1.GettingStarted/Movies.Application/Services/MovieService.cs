using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IValidator<Movie> _movieValidator;


        public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator)
        {
            _movieRepository = movieRepository;
            _movieValidator = movieValidator;
        }

        public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
        {
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
            return await _movieRepository.CreateAsync(movie, cancellationToken);
        }

        public Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return _movieRepository.GetByIdAsync(id, cancellationToken);
        }

        public Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return _movieRepository.GetBySlugAsync(slug, cancellationToken);
        }

        public Task<List<Movie>?> GetAllAsync(CancellationToken cancellationToken)
        {
            return _movieRepository.GetAllAsync(cancellationToken);
        }

        public async Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken)
        {
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
            var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, cancellationToken);

            if(!movieExists)
                return null;

            var result = await _movieRepository.UpdateAsync(movie, cancellationToken);
            if (result)
                return movie;
            else
                return null;
        }

        public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return _movieRepository.DeleteByIdAsync(id, cancellationToken);
        }
    }
}
