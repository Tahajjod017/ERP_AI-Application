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
    public class ActionLogService : AppService<ActionLog>, IActionLogService
    {
        private readonly IGenericRepository<ActionLog> _actionLogs;

        public ActionLogService(IGenericRepository<ActionLog> actionLogs) : base(actionLogs)
        {
            _actionLogs = actionLogs;
        }

        public async Task<PaginationService<ActionLog, ActionLogSetupVM>.PaginationResult<ActionLogSetupVM>> GetPaginateActionLog(
    int pageNumber = 1,
    int pageSize = 5,
    string searchTerm = "",
    string sortColumn = "fdrBnkName",
    string sortOrder = "asc", DateTime? fromDate = null, DateTime? toDate = null, string? tergetType = null, string? actionName = null, int? createdBy = null)
        {
            try
            {
                Console.WriteLine($"From: {fromDate}, To: {toDate}");

                var query = _actionLogs.All().OrderByDescending(x => x.ActionLogId).Where(x =>
    (fromDate == null || x.CreatedAt >= fromDate.Value.Date) &&
    (toDate == null || x.CreatedAt <= toDate.Value.Date) && (string.IsNullOrEmpty(tergetType) || x.TargetType == tergetType) && (string.IsNullOrEmpty(actionName) || x.ActionName == actionName) && (createdBy == null || x.CreatedBy == createdBy)).Include(x => x.CreatedByNavigation);

                if (query == null)
                {
                    throw new InvalidOperationException("ActionLogs query source is null.");
                }

                return await PaginationService<ActionLog, ActionLogSetupVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => b => EF.Functions.Like(b.ActionLogId.ToString(), $"%{term}%") ||
                                 EF.Functions.Like((b.ActionName ?? ""), $"%{term}%") ||
                                 EF.Functions.Like((b.UserEmail ?? ""), $"%{term}%") ||
                                 EF.Functions.Like((b.CreatedByNavigation != null ? (b.CreatedByNavigation.FirstName + " " + b.CreatedByNavigation.LastName) : ""), $"%{term}%") ||
                                 EF.Functions.Like((b.ActionBefore ?? ""), $"%{term}%") ||
                                 EF.Functions.Like((b.ActionAfter ?? ""), $"%{term}%") ||
                                 EF.Functions.Like((b.CreatedBy.ToString() ?? ""), $"%{term}%"),


                    b => new ActionLogSetupVM
                    {
                        ActionLogID = b.ActionLogId,
                        ActionName = b.ActionName ?? "",
                        ActionBefore = b.ActionBefore ?? "",
                        ActionAfter = b.ActionAfter ?? "",
                        UserEmail = b.UserEmail ?? "",
                        CreatedBy = b.CreatedBy ?? 0,
                        CreatedAt = b.CreatedAt ?? DateTime.MinValue,
                        TargetID = b.TargetId ?? 0,
                        TargetType = b.TargetType ?? "",
                        EmployeeUserName = b.CreatedByNavigation != null
                            ? $"{b.CreatedByNavigation.FirstName} {b.CreatedByNavigation.LastName}"
                            : ""
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPaginateActionLog: {ex.Message}");

                return new PaginationService<ActionLog, ActionLogSetupVM>.PaginationResult<ActionLogSetupVM>
                {
                    Data = new List<ActionLogSetupVM>(),
                    TotalCount = 0
                };
            }
        }

    }
}
