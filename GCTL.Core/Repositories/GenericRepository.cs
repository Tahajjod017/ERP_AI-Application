using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GCTL.Core.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }


        #region Queries
        public IQueryable<T> All()
        {
            return _context.Set<T>();
        }

        //public IQueryable<T> AllActive()
        //{
        //    return _context.Set<T>().Where(e => EF.Property<DateTime?>(e, "DeletedAt") == null && EF.Property<int?>(e, "DeletedBy") == null);
        //}

        public IQueryable<T> AllActive()
        {
            var entityType = typeof(T);
            var query = _context.Set<T>().AsQueryable();

            
            // Check if 'DeletedAt' property exists
            var hasDeletedAt = entityType.GetProperty("DeletedAt") != null;
            var hasDeletedBy = entityType.GetProperty("DeletedBy") != null;

            if (hasDeletedAt && hasDeletedBy)
            {
                query = query.Where(e =>EF.Property<DateTime?>(e, "DeletedAt") == null && EF.Property<int?>(e, "DeletedBy") == null);
            }
            else if (hasDeletedAt)
            {
                query = query.Where(e =>EF.Property<DateTime?>(e, "DeletedAt") == null);
            }
            else if (hasDeletedBy)
            {
                query = query.Where(e =>EF.Property<int?>(e, "DeletedBy") == null);
            }

            return query;
        }



        public IQueryable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().Where(expression);
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().Where(expression).ToListAsync();
        }

        //public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        //{
        //    return Task.FromResult(_context.Set<T>().FirstOrDefault(predicate));
        //}
        #endregion


        #region Gets
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        #endregion


        #region Adds
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(T entity, object model)
        {
            if (entity == null || model == null)
                return;

            var modelType = model.GetType();
            var entityType = entity.GetType();

            var propertiesToUpdate = new[] { "CreatedBy", "CreatedAt", "LIP", "LMAC" };

            foreach (var propertyName in propertiesToUpdate)
            {
                var entityProperty = entityType.GetProperty(propertyName);

                // Skip if entity does not have this property or it's not writable
                if (entityProperty == null || !entityProperty.CanWrite)
                    continue;

                object value = null;

                if (propertyName == "CreatedAt")
                {
                    value = DateTime.UtcNow;
                }
                else
                {
                    var modelProperty = modelType.GetProperty(propertyName);
                    if (modelProperty != null)
                    {
                        value = modelProperty.GetValue(model);
                    }
                }

                entityProperty.SetValue(entity, value);
            }

            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }


        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
        #endregion


        #region Updates
        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity, object model)
        {
            if (entity == null || model == null)
                return;

            var modelType = model.GetType();
            var entityType = entity.GetType();

            var propertiesToUpdate = new[] { "UpdatedBy", "UpdatedAt", "LIP", "LMAC" };

            foreach (var propertyName in propertiesToUpdate)
            {
                var entityProperty = entityType.GetProperty(propertyName);

                // Skip if entity does not have this property or it's not writable
                if (entityProperty == null || !entityProperty.CanWrite)
                    continue;

                object value = null;

                if (propertyName == "UpdatedAt")
                {
                    value = DateTime.UtcNow;
                }
                else
                {
                    var modelProperty = modelType.GetProperty(propertyName);
                    if (modelProperty != null)
                    {
                        value = modelProperty.GetValue(model);
                    }
                }

                entityProperty.SetValue(entity, value);
            }

            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().UpdateRange(entities);
            await _context.SaveChangesAsync();
        }
        #endregion


        #region Deletes
        public async Task DeleteAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return;

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(Func<T, bool> predicate)
        {
            var entities = _context.Set<T>().Where(predicate).ToList();
            if(entities.Count == 0) return;

            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }
        #endregion


        #region Transaction
        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            var transaction = _context.Database.CurrentTransaction;
            if (transaction == null) return;

            await transaction.CommitAsync();
            await transaction.DisposeAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            var transaction = _context.Database.CurrentTransaction;
            if (transaction == null) return;

            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
        }

        public List<object> GetDropdown(Expression<Func<T, int>> idSelector, Expression<Func<T, string>> nameSelector, Expression<Func<T, bool>>? filter = null)
        {
            var query = _context.Set<T>().AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query
                .Select(e => new 
                {
                    Id = EF.Property<int>(e, ((MemberExpression)idSelector.Body).Member.Name),
                    Name = EF.Property<string>(e, ((MemberExpression)nameSelector.Body).Member.Name)
                })
                .Cast<object>()
                .ToList();
        }


        public async Task<List<object>> GetDropdownAsync(    Expression<Func<T, int>> idSelector,    Expression<Func<T, string>> nameSelector,    Expression<Func<T, bool>>? filter = null)
        {
            var query = _context.Set<T>().AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query
                .Select(e => new
                {
                    Id = EF.Property<int>(e, ((MemberExpression)idSelector.Body).Member.Name),
                    Name = EF.Property<string>(e, ((MemberExpression)nameSelector.Body).Member.Name)
                })
                .Cast<object>()
                .ToListAsync(); // Asynchronous version of ToList
        }



        #endregion
    }
}
