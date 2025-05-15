using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Pagination
{
    public class PaginationService<TEntity, TViewModel> where TEntity : class
    {
        public class PaginationResult<T>
        {
            public IEnumerable<T> Data { get; set; }
            public int TotalCount { get; set; }
            public PaginationInfo PaginationInfo { get; set; }
        }

        public class PaginationInfo
        {
            public int StartItem { get; set; }
            public int EndItem { get; set; }
            public int TotalItems { get; set; }
            public List<int> PageNumbers { get; set; }
            public int TotalPages { get; set; }
            public int CurrentPage { get; set; }
        }

        // Method to get paginated data
        public static async Task<PaginationResult<TViewModel>> GetPaginatedData(
            IQueryable<TEntity> query,
            int pageNumber,
            int pageSize,
            string searchTerm,
            string sortColumn,
            string sortOrder,
            Func<string, Expression<Func<TEntity, bool>>> searchPredicate,
            Func<TEntity, TViewModel> selector)
        {
            // Apply search filter if search term is provided
            if (!string.IsNullOrEmpty(searchTerm) && searchPredicate != null)
            {
                query = query.Where(searchPredicate(searchTerm));
            }

            // Apply sorting based on the column and direction
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var propertyInfo = typeof(TEntity).GetProperty(sortColumn);
                if (propertyInfo != null)
                {
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(e => EF.Property<object>(e, sortColumn))
                        : query.OrderByDescending(e => EF.Property<object>(e, sortColumn));
                }
            }

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Apply pagination 
            // Was ToListAsync() before
            var paginatedData = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToList();

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var startItem = totalItems == 0 ? 0 : (pageNumber - 1) * pageSize + 1;
            var endItem = Math.Min(pageNumber * pageSize, totalItems);
            var pageNumbers = Enumerable.Range(1, totalPages).ToList();

            return new PaginationResult<TViewModel>
            {
                Data = paginatedData,
                TotalCount = totalItems,
                PaginationInfo = new PaginationInfo
                {
                    StartItem = startItem,
                    EndItem = endItem,
                    TotalItems = totalItems,
                    PageNumbers = pageNumbers,
                    TotalPages = totalPages,
                    CurrentPage = pageNumber
                }
            };
        }
    }
}
