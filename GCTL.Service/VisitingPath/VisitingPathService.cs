using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.ActionLogVM;
using GCTL.Core.ViewModels.VisitingVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.VisitingPath
{
    public class VisitingPathService:AppService<UserVisitLogs>, IVisitingPathService
    {
        private readonly IGenericRepository<UserVisitLogs> _repository;

        public VisitingPathService(IGenericRepository<UserVisitLogs> repository):base(repository) 
        {
            _repository = repository;
        }

        public async Task<PaginationService<UserVisitLogs, UserVisitTreeViewModel>.PaginationResult<UserVisitTreeViewModel>> GetAllAsync(
   int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "", string sortOrder = "")
        {
            try
            {
                var query = _repository.All().AsQueryable();

                // Filter by search term
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.Path.Contains(searchTerm));
                }

                // Group by UserId
                var grouped = query
                    .GroupBy(v => v.UserId)
                    .Select(g => new UserVisitTreeViewModel
                    {
                        UserId = g.Key,
                        Visits = g.Select(v => new UserVisitTreeViewModel.PageVisit
                        {
                            Path = v.Path,
                            VisitTime = v.VisitTime,
                            DurationInSeconds = v.DurationInSeconds ?? 0
                        }).ToList()
                    }).OrderByDescending(x=>x.UserId);

                // Apply Sorting
                grouped = sortColumn switch
                {
                    "UserId" => (sortOrder == "asc") ? grouped.OrderByDescending(x => x.UserId) : grouped.OrderByDescending(x => x.UserId),
                    _ => grouped.OrderByDescending(x => x.UserId) // Default fallback
                };

                // Get total count before pagination
                var totalCount = await grouped.CountAsync();

                // Apply Pagination
                var pagedData = await grouped
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    
                    .ToListAsync();

                return new PaginationService<UserVisitLogs, UserVisitTreeViewModel>.PaginationResult<UserVisitTreeViewModel>
                {
                    Data = pagedData,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
                return new PaginationService<UserVisitLogs, UserVisitTreeViewModel>.PaginationResult<UserVisitTreeViewModel>
                {
                    Data = new List<UserVisitTreeViewModel>(),
                    TotalCount = 0 
                };
            }
        }



    }
}
