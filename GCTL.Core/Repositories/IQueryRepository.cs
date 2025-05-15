using System.Data;

namespace GCTL.Core.Repository
{
    public interface IQueryRepository<T> where T : class
    {
        Task<IEnumerable<T>> QueryAsync(string sql, object? param = null, CommandType commandType = CommandType.Text);
        Task<T?> QuerySingleAsync(string sql, object? param = null, CommandType commandType = CommandType.Text);
    }
}
