using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.HRMsettingsVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GCTL.Service.HRMsettings.ProbationService 
{
    public interface IProbationSettingService
    {
        Task<bool> AddAsync(ProbationSettingVM model);
        Task<bool> UpdateAsync(ProbationSettingVM model);
        Task<ProbationSettingVM> GetByIdAsync(int id);
        Task<PaginationService<ProbetionPeriodSettings, ProbationSettingVM>.PaginationResult<ProbationSettingVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null);
        Task<ProbationSettingVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<bool> IsNameUniqueAsync(string name);
        Task<List<SelectListItem>> GetOrganizationsAsync();
        //Task<List<SelectListItem>> GetApprovalTypesAsync();
        //Task<List<SelectListItem>> GetEmployeeAsync();
        //Task<List<SelectListItem>> GetDesignationAsync();
    }
}
