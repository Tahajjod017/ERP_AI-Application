using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.SystemSettings.ISmsSettingService
{
    public interface ISmsSettingsService
    {
        #region CRUD
        Task<bool> AddAsync(SmsSettingsVM model);
        Task<bool> UpdateAsync(SmsSettingsVM model);
        Task<SmsSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<SmsSettingsVM > GetByIdAsync(int id);
        Task<PaginationService<SMSSettings, SmsSettingsVM>.PaginationResult<SmsSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "SMSSettingID", string sortOrder = "desc", int? organizationID = null);
        #endregion
        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
