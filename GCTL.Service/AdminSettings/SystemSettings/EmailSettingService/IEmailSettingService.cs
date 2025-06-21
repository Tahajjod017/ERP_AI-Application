using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.SystemSettings.Emailsettingservice
{
    public interface IEmailSettingService
    {
        #region CRUD
        Task<bool> AddAsync(EmailSettingsViewModel model);
        Task<bool> UpdateAsync(EmailSettingsViewModel model);
        Task<EmailSettingsViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<EmailSettingsViewModel> GetByIdAsync(int id);
        Task<PaginationService<EmailSettings, EmailSettingsViewModel>.PaginationResult<EmailSettingsViewModel>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "EmailSettingID", string sortOrder = "desc", int? organizationID = null);
        #endregion
        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
    
}
