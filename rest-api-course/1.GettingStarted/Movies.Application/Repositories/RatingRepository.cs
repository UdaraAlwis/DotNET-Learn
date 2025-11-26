using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public RatingRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var result = await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO ratings (userid, movieid, rating)
                VALUES (@userId, @movieId, @rating)
                on conflict (userid, movieid) do update
                    set rating = @rating
                """,
                new { userId, movieId, rating }, cancellationToken: cancellationToken));
            return result > 0;
        }

        public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var result = await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition(
                """
                SELECT ROUND(AVG(r.rating), 1) 
                FROM ratings r
                WHERE movieid = @movieId
                """, new { movieId }, cancellationToken: cancellationToken));
            return result;
        }

        public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var result = await connection.QuerySingleOrDefaultAsync<(float? Rating, int? Count)>(new CommandDefinition(
                """
                SELECT
                    ROUND(AVG(r.rating), 1) AS Rating,
                    (SELECT rating 
                    FROM ratings 
                    WHERE movieid = @movieId 
                        AND userid = @userId
                    LIMIT 1)
                FROM ratings r
                WHERE movieid = @movieId
                """, new { movieId, userId }, cancellationToken: cancellationToken));
            return result;
        }

        public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var result = await connection.ExecuteAsync(new CommandDefinition(
                """
                DELETE FROM ratings
                WHERE movieid = @movieId
                    AND userid = @userId
                """, new { movieId, userId }, cancellationToken: cancellationToken));
            return result > 0;
        }

        public async Task<IEnumerable<MovieRating>> GetUserRatingsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            var result = await connection.QueryAsync<MovieRating>(new CommandDefinition(
                """
                SELECT 
                    m.id AS MovieId,
                    m.slug AS Slug,
                    r.rating AS Rating
                FROM ratings r
                JOIN movies m ON r.movieid = m.id
                WHERE r.userid = @userId
                """, new { userId }, cancellationToken: cancellationToken));
            return result;
        }
    }
}
