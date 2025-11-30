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

        public async Task<List<Movie>?> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var orderClause = string.Empty;
            if (options.SortField is not null)
            {
                orderClause = $"""
                    , m.{options.SortField}
                    ORDER BY m.{options.SortField} 
                    {(options.SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}
                    """;
            }

            var results = await connection.QueryAsync(new CommandDefinition(
                $"""
                SELECT m.*, 
                       STRING_AGG(DISTINCT g.name, ',') AS genres, 
                       ROUND(AVG(r.rating), 1) AS rating, 
                       myr.rating AS userrating
                FROM movies m
                left join genres g ON m.id = g.movieid
                left join ratings r ON m.id = r.movieid
                left join ratings myr ON m.id = myr.movieid
                                    and myr.userid = @userId
                WHERE (@title IS NULL OR m.title ILIKE '%' || @title || '%')
                AND (@yearOfRelease IS NULL OR m.yearofrelease = @yearOfRelease)
                GROUP BY id, userrating {orderClause}
                limit @pageSize
                offset @pageOffset
                """,
                new
                {
                    userId = options.UserId,
                    title = options.Title,
                    yearOfRelease = options.YearOfRelease,
                    pageSize = options.PageSize,
                    pageOffset = (options.Page - 1) * options.PageSize
                }, cancellationToken: cancellationToken));

            return results.Select(x => new Movie()
            {
                Id = x.id,
                Title = x.title,
                YearOfRelease = x.yearofrelease,
                Rating = (float?)x.rating,
                UserRating = (int?)x.userrating,
                Genres = Enumerable.ToList(x.genres.Split(',')),
            }).ToList();
        }

        public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition(
                """
                SELECT m.*, ROUND(AVG(r.rating), 1) as rating, myr.rating as userrating
                FROM movies m
                left join ratings r ON m.id = r.movieid
                left join ratings myr ON m.id = myr.movieid and myr.userid = @userId
                WHERE id = @id
                group by id, userrating
                """, new { id, userId }, cancellationToken: cancellationToken));

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

        public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
        {
            var connectionFactory = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var movie = await connectionFactory.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition(
                """
                SELECT m.*, ROUND(AVG(r.rating), 1) as rating, myr.rating as userrating
                FROM movies m
                left join ratings r ON m.id = r.movieid
                left join ratings myr ON m.id = myr.movieid and myr.userid = @userId
                WHERE slug = @slug
                group by id, userrating
                """, new { slug, userId }, cancellationToken: cancellationToken));

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

        public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var result = await connection.QuerySingleAsync<int>(new CommandDefinition("""
                SELECT COUNT(id) FROM movies
                WHERE (@title IS NULL OR title LIKE '%' || @title || '%')
                AND (@yearOfRelease IS NULL OR yearofrelease = @yearOfRelease)
                """, 
                new { title, yearOfRelease },
                cancellationToken: cancellationToken));
            return result;
        }
    }
}
