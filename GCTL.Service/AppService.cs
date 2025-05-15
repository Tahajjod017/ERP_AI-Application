using GCTL.Core.Repository;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service
{
    public class AppService<T> where T : class
    {
        private readonly IGenericRepository<T> _genericRepository;

        public AppService(IGenericRepository<T> genericRepository)
        {
            _genericRepository = genericRepository;
        }


        #region CRUD Operations
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _genericRepository.GetAllAsync();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await _genericRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _genericRepository.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _genericRepository.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(T entity)
        {
            await _genericRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(object id)
        {
            await _genericRepository.DeleteAsync(id);
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _genericRepository.FindAsync(expression);
        }

        #endregion
    }
}
