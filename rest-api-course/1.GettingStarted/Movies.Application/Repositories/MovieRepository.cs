using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public MovieRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this._dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            var result = await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO movies (id, title, yearofrelease, slug) 
                VALUES (@Id, @Title, @YearOfRelease, @Slug)
                """, movie, cancellationToken: cancellationToken));

            if (result > 0)
            {
                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO genres (movieId, name) 
                    VALUES (@MovieId, @Name)
                    """,
                    new
                    {
                        MovieId = movie.Id,
                        Name = genre
                    }, cancellationToken: cancellationToken));
                }
            }

            transaction.Commit();

            return result > 0;
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieid = @MovieId
                """,
                new { MovieId = id }, cancellationToken: cancellationToken));

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM movies WHERE id = @MovieId
                """,
                new { MovieId = id }, cancellationToken: cancellationToken));

            transaction.Commit();

            return result > 0;
        }

        public async Task<List<Movie>?> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var results = await connection.QueryAsync(new CommandDefinition(
                """
                SELECT m.*, STRING_AGG(g.name, ',') as genres
                FROM movies m
                left join genres g ON m.id = g.movieid
                GROUP BY id
                """,
                cancellationToken: cancellationToken));

            return results.Select(x => new Movie()
            {
                Id = x.id,
                Title = x.title,
                YearOfRelease = x.yearofrelease,
                Genres = Enumerable.ToList(x.genres.Split(',')),
            }).ToList();
        }

        public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition(
                """
                SELECT * FROM movies WHERE id = @id
                """, new { id }, cancellationToken: cancellationToken));


            if (movie == null)
                return null;

            var genres = await connection.QueryAsync<string>(new CommandDefinition(
                """
                SELECT name FROM genres WHERE movieid = @movieId
                """, new { movie.Id }, cancellationToken: cancellationToken));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }

        public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var connectionFactory = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var movie = await connectionFactory.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition(
                "SELECT * FROM movies WHERE slug = @slug", new { slug }, cancellationToken: cancellationToken));

            if (movie == null)
                return null;

            var genres = await connectionFactory.QueryAsync<string>(new CommandDefinition(
                "SELECT name from genres where movieid = @movieId", new { movieId = movie.Id }, 
                cancellationToken: cancellationToken));

            foreach (var item in genres)
            {
                movie.Genres.Add(item);
            }

            return movie;
        }

        public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieid = @MovieId
                """,
                new { MovieId = movie.Id }, cancellationToken: cancellationToken));

            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movieid, name) 
                    VALUES (@MovieId, @Name)
                    """,
                    new
                    {
                        MovieId = movie.Id,
                        Name = genre
                    }, cancellationToken: cancellationToken));
            }

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE movies 
                SET title = @Title, yearofrelease = @YearOfRelease, slug = @Slug
                WHERE id = @Id
                """, movie, cancellationToken: cancellationToken));

            transaction.Commit();

            return result > 0;
        }

        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT COUNT(1) FROM movies WHERE id = @id
                """, new { id }, cancellationToken: cancellationToken));
        }
    }
}
