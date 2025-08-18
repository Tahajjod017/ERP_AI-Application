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

namespace GCTL.Service.AdminSettings.GeneralSettings
{
    public interface ILocalizationSettingService
    {

        Task<bool> AddAsync(LocalizationViewModel model);
        Task<bool> UpdateAsync(LocalizationViewModel model);
        // Task<bool> DeleteAsync(int id);
        Task<LocalizationViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);
        // Task<bool> IsLocalizationUniqueAsync(int? organizationId, int? languageId, int? timezoneId);
        Task<LocalizationViewModel> GetByIdAsync(int id);
        // Task<LocalizationViewModel> GetByIdAsync(int id);
        Task<PaginationService<Localizations, LocalizationViewModel>.PaginationResult<LocalizationViewModel>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
            string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null);
        Task<Localizations> GetForOrganizationAsync(int orgId);
        Task<List<SelectListItem>> GetOrganizationsAsync();
        Task<List<SelectListItem>> GetLanguagesAsync();
        Task<List<SelectListItem>> GetTimeformatAsync();
        Task<List<SelectListItem>> GetTimeZoneAsync();
        Task<List<SelectListItem>> GetDateFormateAsync();
        Task<List<SelectListItem>> GetCurrencieAsync();

        // You can also declare other helpers you’re already using in middleware:
        Task<string> GetIanaTimeZoneByIdAsync(int? timezoneId);
        Task<string> GetDatePatternByIdAsync(int? dateFormatId);
        Task<string> GetTimePatternByIdAsync(int? timeFormatId);

        // Optional: one-shot bundle to reduce DB round-trips
        Task<OrgLocBundle> GetOrgLocalizationBundleAsync(int orgId);

    }
}
