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
        private readonly IRatingRepository _ratingRepository;
        private readonly IValidator<Movie> _movieValidator;


        public MovieService(IMovieRepository movieRepository, IRatingRepository ratingRepository, IValidator<Movie> movieValidator)
        {
            _movieRepository = movieRepository;
            _ratingRepository = ratingRepository;
            _movieValidator = movieValidator;
        }

        public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
        {
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
            return await _movieRepository.CreateAsync(movie, cancellationToken);
        }

        public Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
        {
            return _movieRepository.GetByIdAsync(id, userId, cancellationToken);
        }

        public Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
        {
            return _movieRepository.GetBySlugAsync(slug, userId, cancellationToken);
        }

        public Task<List<Movie>?> GetAllAsync(Guid? userId = default, CancellationToken cancellationToken = default)
        {
            return _movieRepository.GetAllAsync(userId, cancellationToken);
        }

        public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken cancellationToken = default)
        {
            await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
            var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, cancellationToken);

            if(!movieExists)
                return null;

            await _movieRepository.UpdateAsync(movie, cancellationToken);

            if (!userId.HasValue)
            {
                var rating = await _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
                movie.Rating = rating;
                return movie;
            }

            var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, cancellationToken);
            movie.Rating = ratings.Rating;
            movie.UserRating = ratings.UserRating;

            return movie;
        }

        public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return _movieRepository.DeleteByIdAsync(id, cancellationToken);
        }
    }
}
