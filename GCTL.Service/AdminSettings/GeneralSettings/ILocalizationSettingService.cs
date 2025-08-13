using GCTL.Core.ViewModels.AdminSettingsVM;
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
        Task<List<SelectListItem>> GetOrganizationsAsync();
        Task<List<SelectListItem>> GetTimeformatAsync();
        Task<List<SelectListItem>> GetTimeZoneAsync();
        Task<List<SelectListItem>> GetDateFormateAsync();
        Task<List<SelectListItem>> GetCurrencieAsync();

    }
}
