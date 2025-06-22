using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GCTL.Service.AdminSettings.OrganizationSettings.HolidayService
{
    public interface IHolidaySettingService
    {
        Task<bool> AddAsync(HolidayViewModel model);
        Task<bool> UpdateAsync(HolidayViewModel model);
        Task<HolidayViewModel> GetByIdAsync(int id);
        Task<PaginationService<Holidays, HolidayViewModel>.PaginationResult<HolidayViewModel>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null);
        Task<HolidayViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<bool> IsNameUniqueAsync(string name);
        Task<List<SelectListItem>> GetOrganizationsAsync();
        Task<List<SelectListItem>> GetHolidayStatusesAsync(); // Optional

    }
}
