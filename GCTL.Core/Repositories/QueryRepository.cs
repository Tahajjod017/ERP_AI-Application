using Dapper;
using System.Data;

namespace GCTL.Core.Repository
{
    public class QueryRepository<T> : IQueryRepository<T> where T : class
    {
        private readonly IDbConnection _connection;

        public QueryRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<T>> QueryAsync(string sql, object? param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.QueryAsync<T>(sql, param, commandType: commandType);
        }

        public async Task<T?> QuerySingleAsync(string sql, object? param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.QuerySingleOrDefaultAsync<T>(sql, param, commandType: commandType);
        }
    }
}
