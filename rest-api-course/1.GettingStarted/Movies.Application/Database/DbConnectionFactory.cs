using Npgsql;
using System.Data;
using System.Threading;

namespace Movies.Application.Database
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
    }

    public class NgpsqlConnectionFactory : IDbConnectionFactory
    {
        private string _connectionString = string.Empty;
        public NgpsqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
