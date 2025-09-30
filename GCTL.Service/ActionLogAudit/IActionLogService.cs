using GCTL.Core.ViewModels.ActionLogVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ActionLogAudit
{
    public interface IActionLogService
    {

        Task<PaginationService<ActionLogs, ActionLogSetupVM>.PaginationResult<ActionLogSetupVM>> GetPaginateActionLog(int pageNumber = 1, int pageSize = 5, string searchTerm = "",

     string currentSortColumn = "", string currentSortOrder = "", DateTime? fromDate = null, DateTime? toDate = null, string? tergetType = null, string? actionName = null, int? createdBy = null);
        Task<PaginationService<ActionLogs, ActionLogSetupVM>.PaginationResult<ActionLogSetupVM>> GetPaginateActionLogException(int pageNumber = 1, int pageSize = 5, string searchTerm = "",

    string currentSortColumn = "", string currentSortOrder = "", DateTime? fromDate = null, DateTime? toDate = null, string? tergetType = null, string? actionName = null, int? createdBy = null);

    }
}
