using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.ActionTakens
{
    public interface IActionTakenService
    {
        #region CRUD
        Task<bool> AddAsync(ActionTakenVM model);
        Task<bool> UpdateAsync(ActionTakenVM model);
        Task<ActionTakenVM> SoftDeleteAsync(List<int> ids);
        Task<ActionTakenVM> GetByIdAsync(int id);
        Task<PaginationService<ActionTaken, ActionTakenVM>.PaginationResult<ActionTakenVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ActionTakenID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion


        #region IP & Mac Address
        string GetLocalIP();
        string GetMacAddress();
        #endregion
    }
}
