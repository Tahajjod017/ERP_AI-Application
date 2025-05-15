using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;

namespace GCTL.Service.Pagination
{

    public class PaginationResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
    }

    public interface IPaginationService
    {
        PaginationResult<T> CreatePaginatedResult<T>(
            IEnumerable<T> source,
            int page,
            int pageSize,
            string searchTerm = "",
            Func<T, string, bool> searchPredicate = null,
            string sortColumn = "",
            string sortDirection = "asc",
            Dictionary<string, Expression<Func<T, object>>> sortColumnMappings = null);
    }
}
