using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Core.ViewModels.VisitingVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.VisitingPath
{
    public interface IVisitingPathService
    {
        Task<PaginationService<UserVisitLogs, UserVisitTreeViewModel>.PaginationResult<UserVisitTreeViewModel>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "", string sortOrder = "");
    }
}
