using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GCTL.Service.AdminSettings.GeneralSettings
{
    public class LocalizationSettingService : AppService<Localizations>, ILocalizationSettingService
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Localizations> _genericRepository;
        private readonly IGenericRepository<Organization> _genericRepositoryOraganization;
        private readonly IGenericRepository<Currencies> _genericRepositoryCurrencies;
        private readonly IGenericRepository<TimeFormats> _genericRepositoryTimeformat;
        private readonly IGenericRepository<Timezones> _genericRepositoryTimeZone;
        private readonly IGenericRepository<DateFormats> _genericRepositoryDateFormat;

        public LocalizationSettingService(IUserInfoService userInfoService, IGenericRepository<Localizations> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization, IGenericRepository<Currencies> genericRepositoryCurrencies, IGenericRepository<TimeFormats> genericRepositoryTimeformat, IGenericRepository<Timezones> genericRepositoryTimeZone, IGenericRepository<DateFormats> genericRepositoryDateFormat) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
            _genericRepositoryCurrencies = genericRepositoryCurrencies;
            _genericRepositoryTimeformat = genericRepositoryTimeformat;
            _genericRepositoryTimeZone = genericRepositoryTimeZone;
            _genericRepositoryDateFormat = genericRepositoryDateFormat;
        }

        #region AddAsync
        public async Task<bool> AddAsync(LocalizationViewModel model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Check if the localization already exists (soft-deleted records)
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.LocalizationID == model.LocalizationID
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity != null)
                {
                    // Restore and update the record
                    existingEntity.OrganizationID = model.OrganizationID;
                    //existingEntity.LanguageID = model.LanguageID;
                    existingEntity.TimezoneID = model.TimezoneID;
                    existingEntity.DateFormatID = model.DateFormatID;
                   // existingEntity.TimeFormatID = model.TimeFormatID;
                    existingEntity.CurrencyID = model.CurrencyID;
                   // existingEntity.CurrencySymbol = model.CurrencySymbol;

                    existingEntity.UpdatedAt = DateTime.Now;
                    existingEntity.UpdatedBy = model.UpdatedBy ?? null;
                    existingEntity.DeletedAt = null;

                    await _genericRepository.UpdateAsync(existingEntity);
                }
                else
                {
                    // Create a new localization entry
                    var newEntity = new Localizations
                    {
                        OrganizationID = model.OrganizationID,
                       // LanguageID = model.LanguageID,
                        TimezoneID = model.TimezoneID,
                        DateFormatID = model.DateFormatID,
                        //TimeFormatID = model.TimeFormatID,
                        CurrencyID = model.CurrencyID,
                        //CurrencySymbol = model.CurrencySymbol,

                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                    };

                    await _genericRepository.AddAsync(newEntity);
                }

                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion


        public async Task<List<SelectListItem>> GetOrganizationsAsync()
        {
            var organizations = await _genericRepositoryOraganization.All()
                .Where(o => o.DeletedAt == null)
                .Select(o => new SelectListItem
                {
                    Value = o.OrganizationID.ToString(),
                    Text = o.OrganizationName
                })
                .ToListAsync();
            // Set first item as selected by default if any exist
            if (organizations.Any())
            {
                organizations[0].Selected = true;
            }



            return organizations;
        }
        public async Task<List<SelectListItem>> GetTimeformatAsync()
        {
            var timeFormats = await _genericRepositoryTimeformat.All()

                .Select(t => new SelectListItem
                {
                    Value = t.TimeFormatID.ToString(),
                    Text = t.DisplayText,
                })
                .ToListAsync();
            // Set first item as selected by default if any exist
            if (timeFormats.Any())
            {
                timeFormats[0].Selected = true;
            }
            return timeFormats;
        }
        public async Task<List<SelectListItem>> GetTimeZoneAsync()
        {
            var timeZones = await _genericRepositoryTimeZone.All()
                .Select(tz => new SelectListItem
                {
                    Value = tz.TimezoneID.ToString(),
                    Text = tz.TimezoneName,
                })
                .ToListAsync();
            // Set first item as selected by default if any exist
            if (timeZones.Any())
            {
                timeZones[0].Selected = true;
            }
            return timeZones;
        }
        public async Task<List<SelectListItem>> GetDateFormateAsync()
        {
            var dateFormats = await _genericRepositoryDateFormat.All()
                .Select(df => new SelectListItem
                {
                    Value = df.DateFormatID.ToString(),
                    Text = df.Description,
                })
                .ToListAsync();
            // Set first item as selected by default if any exist
            if (dateFormats.Any())
            {
                dateFormats[0].Selected = true;
            }
            return dateFormats;
        }
        public async Task<List<SelectListItem>> GetCurrencieAsync()
        {
            var currencies = await _genericRepositoryCurrencies.All()
                .Select(c => new SelectListItem
                {
                    Value = c.CurrencyID.ToString(),
                    Text = c.CurrencyName,
                })
                .ToListAsync();
            // Set first item as selected by default if any exist
            if (currencies.Any())
            {
                currencies[0].Selected = true;
            }
            return currencies;
        }
    }
}
