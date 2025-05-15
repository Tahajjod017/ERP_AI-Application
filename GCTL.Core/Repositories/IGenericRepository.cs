using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Core.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        #region Queries
        IQueryable<T> All();
        IQueryable<T> AllActive();
        IQueryable<T> Find(Expression<Func<T, bool>> expression);
        Task<List<T>> FindAsync(Expression<Func<T, bool>> expression);
        //Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        #endregion


        #region Gets
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id);
        #endregion


        #region Adds
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        #endregion


        #region Updates
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        #endregion


        #region Deletes
        Task DeleteAsync(object id);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task RemoveRangeAsync(Func<T, bool> predicate);
        #endregion


        #region Transactions
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        #endregion


        #region DropDown
        // 👇 
        List<object> GetDropdown(Expression<Func<T, int>> idSelector, Expression<Func<T, string>> nameSelector, Expression<Func<T, bool>>? filter = null);
        Task<List<object>> GetDropdownAsync(Expression<Func<T, int>> idSelector, Expression<Func<T, string>> nameSelector, Expression<Func<T, bool>>? filter = null);
        #endregion
    }
}
