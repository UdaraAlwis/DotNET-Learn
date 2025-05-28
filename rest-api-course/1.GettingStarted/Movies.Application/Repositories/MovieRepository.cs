using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;
using System.Data;
using System.Data.Common;

namespace Movies.Application.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public MovieRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this._dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> CreateMovieAsync(Movie movie)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            var result = await connection.ExecuteAsync(
                "INSERT INTO movies (id, title, yearofrelease, slug) " +
                "VALUES (@Id, @Title, @YearOfRelease, @Slug)",
                movie);

            if (result > 0)
            {
                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync(
                    "INSERT INTO genres (movieId, name) " +
                    "VALUES (@MovieId, @Name)",
                    new
                    {
                        MovieId = movie.Id,
                        Name = genre
                    });
                }
            }

            transaction.Commit();

            return result > 0;
        }

        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Movie>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Movie?> GetMovieByIdAsync(Guid id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition(
                "SELECT * FROM movies WHERE id = @id", new { id }));

            if (movie == null)
                return null;

            var genres = await connection.QueryAsync<string>(new CommandDefinition(
                "SELECT name from genres where movieid = @movieId", new { movie.Id }));

            foreach (var item in genres)
            {
                movie.Genres.Add(item);
            }

            return movie;
        }

        public Task<Movie?> GetMovieBySlugAsync(string slug)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateMovieAsync(Movie movie)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
