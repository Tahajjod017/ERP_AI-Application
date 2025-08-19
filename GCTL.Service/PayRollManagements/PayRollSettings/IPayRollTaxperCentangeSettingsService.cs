using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollSettings;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollSettings
{
    public interface IPayRollTaxperCentangeSettingsService
    {
        #region CRUD
        Task<bool> AddAsync(PayRollTaxPercentageSaveVM model);
        Task<bool> UpdateAsync(PayRollTaxpercentageUpdateVM model);
        Task<PayRollTaxpercentageUpdateVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<PayRollTaxpercentageUpdateVM> GetByIdAsync(int id);
        Task<PaginationService<PSettings, PayRollTaxpercentageGetAllVM>.PaginationResult<PayRollTaxpercentageGetAllVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
