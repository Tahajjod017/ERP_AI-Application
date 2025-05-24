using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.ActionLogVM;
using GCTL.Core.ViewModels.VisitingVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.Data.SqlClient;
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
          string currentSortColumn = "", string currentSortOrder = "")
        {
            try
            {
                var query = _repository.All().Where(x=>x.VisitTime !=null).OrderByDescending(x=>x.UserId).AsQueryable();

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
                            DurationInSeconds = Math.Round(v.DurationInSeconds ?? 0, 2)
                        }).ToList()
                    }).OrderByDescending(x => x.UserId);

                // Apply Sorting
                grouped = currentSortColumn switch
                {
                    "UserId" => (currentSortOrder == "asc") ? grouped.OrderBy(x => x.UserId) : grouped.OrderByDescending(x => x.UserId),
                    _ => grouped.OrderByDescending(x => x.UserId) // Default fallback
                };

                // Get total count before pagination
                var totalCount = await grouped.CountAsync();

                // Apply Pagination
                var pagedData = await grouped
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Construct PaginationInfo
                var paginationInfo = new PaginationService<UserVisitLogs, UserVisitTreeViewModel>.PaginationInfo
                {
                    StartItem = totalCount == 0 ? 0 : ((pageNumber - 1) * pageSize) + 1,
                    EndItem = Math.Min(pageNumber * pageSize, totalCount),
                    TotalItems = totalCount,
                    PageNumbers = Enumerable.Range(1, (int)Math.Ceiling((double)totalCount / pageSize)).ToList(),
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    CurrentPage = pageNumber
                };

                return new PaginationService<UserVisitLogs, UserVisitTreeViewModel>.PaginationResult<UserVisitTreeViewModel>
                {
                    Data = pagedData,
                    TotalCount = totalCount,
                    PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
                return new PaginationService<UserVisitLogs, UserVisitTreeViewModel>.PaginationResult<UserVisitTreeViewModel>
                {
                    Data = new List<UserVisitTreeViewModel>(),
                    TotalCount = 0,
                    PaginationInfo = new PaginationService<UserVisitLogs, UserVisitTreeViewModel>.PaginationInfo
                    {
                        StartItem = 0,
                        EndItem = 0,
                        TotalItems = 0,
                        PageNumbers = new List<int>(),
                        TotalPages = 0,
                        CurrentPage = pageNumber
                    }
                };
            }
        }

        //

        //


    }
}
