using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;

namespace GCTL.Service.Pagination
{
    public class PaginationService : IPaginationService
    {
        public PaginationResult<T> CreatePaginatedResult<T>(
            IEnumerable<T> source,
            int page,
            int pageSize,
            string searchTerm = "",
            Func<T, string, bool> searchPredicate = null,
            string sortColumn = "",
            string sortDirection = "asc",
            Dictionary<string, Expression<Func<T, object>>> sortColumnMappings = null)
        {
            // Apply search if search term and predicate are provided
            if (!string.IsNullOrEmpty(searchTerm) && searchPredicate != null)
            {
                source = source.Where(item => searchPredicate(item, searchTerm));
            }

            // Apply sorting if sort column and mappings are provided
            if (!string.IsNullOrEmpty(sortColumn) && sortColumnMappings != null && sortColumnMappings.ContainsKey(sortColumn))
            {
                var keySelector = sortColumnMappings[sortColumn].Compile();
                source = sortDirection.ToLower() == "asc"
                    ? source.OrderBy(keySelector)
                    : source.OrderByDescending(keySelector);
            }

            // Get total count before pagination
            int totalRecords = source.Count();

            // Apply pagination
            var data = source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Return paginated result
            return new PaginationResult<T>
            {
                Data = data,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
