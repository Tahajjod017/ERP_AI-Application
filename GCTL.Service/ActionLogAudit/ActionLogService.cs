using GCTL.Core.Repository;
using GCTL.Core.ViewModels.ActionLogVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ActionLogAudit
{
    public class ActionLogService : AppService<ActionLogs>, IActionLogService
    {
        private readonly IGenericRepository<ActionLogs> _actionLogs;

        public ActionLogService(IGenericRepository<ActionLogs> actionLogs) : base(actionLogs)
        {
            _actionLogs = actionLogs;
        }

        public async Task<PaginationService<ActionLogs, ActionLogSetupVM>.PaginationResult<ActionLogSetupVM>> GetPaginateActionLog(
    int pageNumber = 1,
    int pageSize = 5,
    string searchTerm = "",
    string currentSortColumn = "",
    string currentSortOrder = "", DateTime? fromDate = null, DateTime? toDate = null, string? tergetType = null, string? actionName = null, int? createdBy = null)
        {
            try
            {
                Console.WriteLine($"From: {fromDate}, To: {toDate}");

                var query = _actionLogs.All().OrderByDescending(x => x.ActionLogID).Include(x => x.CreatedByNavigation).Where(x =>
    (fromDate == null || x.CreatedAt >= fromDate.Value.Date) &&
    (toDate == null || x.CreatedAt <= toDate.Value.Date) && (string.IsNullOrEmpty(tergetType) || x.TargetType == tergetType) && (string.IsNullOrEmpty(actionName) || x.ActionName == actionName) && (createdBy == null || x.CreatedBy == createdBy));

                if (query == null)
                {
                    throw new InvalidOperationException("ActionLogs query source is null.");
                }

                if (!string.IsNullOrEmpty(currentSortColumn))
                {
                    bool ascending = currentSortOrder?.ToLower() == "asc";

                    query = currentSortColumn switch
                    {
                        "ActionLogID" => ascending ? query.OrderBy(x => x.ActionLogID) : query.OrderByDescending(x => x.ActionLogID),
                        "EmployeeUserName" => ascending ? query.OrderBy(x => x.CreatedByNavigation.FirstName).ThenBy(x => x.CreatedByNavigation.LastName)
                                                        : query.OrderByDescending(x => x.CreatedByNavigation.FirstName).ThenByDescending(x => x.CreatedByNavigation.LastName),
                        "UserEmail" => ascending ? query.OrderBy(x => x.UserEmail) : query.OrderByDescending(x => x.UserEmail),
                        "TargetType" => ascending ? query.OrderBy(x => x.TargetType) : query.OrderByDescending(x => x.TargetType),
                        "ActionName" => ascending ? query.OrderBy(x => x.ActionName) : query.OrderByDescending(x => x.ActionName),
                        _ => query
                    };
                }


                var result = await PaginationService<ActionLogs, ActionLogSetupVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                    term => b => EF.Functions.Like(b.ActionLogID.ToString(), $"%{term}%") ||
                                 EF.Functions.Like((b.ActionName ?? ""), $"%{term}%") ||
                                 EF.Functions.Like((b.UserEmail ?? ""), $"%{term}%") ||
                                 EF.Functions.Like((b.CreatedByNavigation != null ? (b.CreatedByNavigation.FirstName + " " + b.CreatedByNavigation.LastName) : ""), $"%{term}%") ||
                                 EF.Functions.Like((b.ActionBefore ?? ""), $"%{term}%") ||
                                 EF.Functions.Like((b.ActionAfter ?? ""), $"%{term}%") ||
                                 EF.Functions.Like((b.CreatedBy.ToString() ?? ""), $"%{term}%"),


                    b => new ActionLogSetupVM
                    {
                        ActionLogID = b.ActionLogID,
                        ActionName = b.ActionName ?? "",
                        ActionBefore = b.ActionBefore ?? "",
                        ActionAfter = b.ActionAfter ?? "",
                        UserEmail = b.UserEmail ?? "",
                        CreatedBy = b.CreatedBy ?? 0,
                        CreatedAt = b.CreatedAt ?? DateTime.MinValue,
                        TargetID = b.TargetID ?? 0,
                        TargetType = b.TargetType ?? "",
                        EmployeeUserName = b.CreatedByNavigation != null
                            ? $"{b.CreatedByNavigation.FirstName} {b.CreatedByNavigation.LastName}"
                            : ""
                    });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPaginateActionLog: {ex.Message}");

                return new PaginationService<ActionLogs, ActionLogSetupVM>.PaginationResult<ActionLogSetupVM>
                {
                    Data = new List<ActionLogSetupVM>(),
                    TotalCount = 0
                };
            }
        }

    }
}
