using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Database
{
    public class DbInitializer
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            this._dbConnectionFactory = dbConnectionFactory;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS movies (
                    id UUID PRIMARY KEY,
                    slug TEXT NOT NULL,
                    title TEXT NOT NULL,
                    yearofrelease INTEGER NOT NULL);"
                );

            await connection.ExecuteAsync(@"
                CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS movies_slug_idx ON movies using btree (slug);");

            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS genres (
                    movieId UUID REFERENCES movies(Id),
                    name TEXT NOT NULL);"
               );

            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS ratings (
                    userid UUID,
                    movieid UUID REFERENCES movies(id),
                    rating INTEGER NOT NULL,
                    PRIMARY KEY(userid, movieid));
                """);
        }
    }
}
