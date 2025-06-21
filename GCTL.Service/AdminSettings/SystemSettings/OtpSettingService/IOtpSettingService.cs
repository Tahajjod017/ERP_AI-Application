using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.SystemSettings.OtpSettingService
{
    public interface IOtpSettingService
    {
        #region CRUD
        Task<bool> AddAsync(OtpSettingsVM model);
        Task<bool> UpdateAsync(OtpSettingsVM model);
        Task<OtpSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<OtpSettingsVM> GetByIdAsync(int id);
        Task<PaginationService<OTPSettings, OtpSettingsVM>.PaginationResult<OtpSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
            string sortColumn = "OtpSettingID", string sortOrder = "desc", int? organizationID = null);
        #endregion
        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
