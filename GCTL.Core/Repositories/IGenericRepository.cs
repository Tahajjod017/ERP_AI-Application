using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
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

        Task AddAsync(T entity, object model);
        Task AddRangeAsync(IEnumerable<T> entities);
        #endregion


        #region Updates
        Task UpdateAsync(T entity);
        Task UpdateAsync(T entity, object model);
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

        #region SelectList Interface Methods

        SelectList GetSelectListById(        Expression<Func<T, int>> idSelector,        Expression<Func<T, string>> nameSelector,        Expression<Func<T, bool>>? filter = null,        object selectedValue = null);

        Task<SelectList> GetSelectListByIdAsync(            Expression<Func<T, int>> idSelector,            Expression<Func<T, string>> nameSelector,            Expression<Func<T, bool>>? filter = null,            object selectedValue = null);

        SelectList GetSelectListByIdAlt(            Expression<Func<T, int>> idSelector,            Expression<Func<T, string>> nameSelector,            Expression<Func<T, bool>>? filter = null,            object selectedValue = null);

        SelectList GetActiveSelectListById(            Expression<Func<T, int>> idSelector,            Expression<Func<T, string>> nameSelector,            Expression<Func<T, bool>>? additionalFilter = null,            object selectedValue = null);

        Task<SelectList> GetActiveSelectListByIdAsync(            Expression<Func<T, int>> idSelector,            Expression<Func<T, string>> nameSelector,            Expression<Func<T, bool>>? additionalFilter = null,            object selectedValue = null);


        #endregion

    }
}
