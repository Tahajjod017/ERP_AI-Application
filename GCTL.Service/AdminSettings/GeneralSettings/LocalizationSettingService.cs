using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
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
        private readonly IGenericRepository<LanguageLists> _genericRepositoryLanguageLists;

        public LocalizationSettingService(IUserInfoService userInfoService, IGenericRepository<Localizations> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization, IGenericRepository<Currencies> genericRepositoryCurrencies, IGenericRepository<TimeFormats> genericRepositoryTimeformat, IGenericRepository<Timezones> genericRepositoryTimeZone, IGenericRepository<DateFormats> genericRepositoryDateFormat, IGenericRepository<LanguageLists> genericRepositoryLanguageLists) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
            _genericRepositoryCurrencies = genericRepositoryCurrencies;
            _genericRepositoryTimeformat = genericRepositoryTimeformat;
            _genericRepositoryTimeZone = genericRepositoryTimeZone;
            _genericRepositoryDateFormat = genericRepositoryDateFormat;
            _genericRepositoryLanguageLists = genericRepositoryLanguageLists;
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
                    existingEntity.LanguageID = model.LanguageID;
                    existingEntity.TimezoneID = model.TimezoneID;
                    existingEntity.DateFormatID = model.DateFormatID;
                    existingEntity.TimeFormatID = model.TimeFormatID;
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

        #region UpdateAsync
        public async Task<bool> UpdateAsync(LocalizationViewModel model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Fetch the existing localization record
                var existingEntity = await _genericRepository.FirstOrDefaultAsync(x => x.LocalizationID == model.LocalizationID && x.DeletedAt == null);
                if (existingEntity == null)
                {
                    throw new InvalidOperationException($"Localization with ID {model.LocalizationID} not found or is soft-deleted.");
                }
                // Update the fields
                existingEntity.OrganizationID = model.OrganizationID;
                existingEntity.LanguageID = model.LanguageID;
                existingEntity.TimezoneID = model.TimezoneID;
                existingEntity.DateFormatID = model.DateFormatID;
                existingEntity.TimeFormatID = model.TimeFormatID;
                existingEntity.CurrencyID = model.CurrencyID;
                // existingEntity.CurrencySymbol = model.CurrencySymbol;
                existingEntity.UpdatedAt = DateTime.Now;
                existingEntity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(existingEntity);

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

        #region delete 

        public async Task<LocalizationViewModel> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.LocalizationID));
                if (data == null || data.Count == 0)
                {
                    return new LocalizationViewModel
                    {
                        Message = "No data found to soft delete."
                    };
                }

                //var beforeEntity = JsonConvert.DeserializeObject<List<HolidayViewModel>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.LocalizationID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    //item.LIP = requestVM.LIP;
                    //item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                //await _userInfoService.ActionLogDeleteAsync("Holiday", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new LocalizationViewModel
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion

        #region getById
        public async Task<LocalizationViewModel> GetByIdAsync(int id)
        {
            var entity = await _genericRepository.FirstOrDefaultAsync(x => x.LocalizationID == id && x.DeletedAt == null);
            if (entity == null)
            {
                return null; // or throw an exception if preferred
            }
            return new LocalizationViewModel
            {
                LocalizationID = entity.LocalizationID,
                OrganizationID = entity.OrganizationID,
                OrganizationName = entity.Organization?.OrganizationName ?? "_", // Assuming OrganizationName exists in Organization
                LanguageID = entity.LanguageID,
                LanguageName = entity.Language?.LanguageName ?? "_", // Assuming LanguageName exists in Language
                TimezoneID = entity.TimezoneID,
                TimezoneName = entity.Timezone?.TimezoneName ?? "_", // Assuming TimezoneName exists in Timezone
                DateFormatID = entity.DateFormatID,
                DateFormat = entity.DateFormat?.Description ?? "_", // Assuming Description exists in DateFormat
                TimeFormatID = entity.TimeFormatID,
                TimeFormat = entity.TimeFormat?.DisplayText ?? "_", // Assuming DisplayText exists in TimeFormat
                CurrencyID = entity.CurrencyID,
                CurrencyName = entity.Currency?.CurrencyName ?? "_", // Assuming CurrencyName exists in Currency
                                                                     // CurrencySymbol = entity.Currency?.CurrencySymbol ?? "_", // Assuming CurrencySymbol exists in Currency
               // CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
               // UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
                // DeletedAt = entity.DeletedAt,
                LIP = entity.LIP,
                LMAC = entity.LMAC
            };
        }
        #endregion
        #region GetAllAsync
        public async Task<PaginationService<Localizations, LocalizationViewModel>.PaginationResult<LocalizationViewModel>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "OrganizationID",
            string sortOrder = "desc",
            int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Organization) // Include related Organization entity
                .Include(x => x.Language) // Include related Language entity
                .Include(x => x.Timezone) // Include related Timezone entity
                .Include(x => x.DateFormat) // Include related DateFormat entity
                .Include(x=>x.TimeFormat)
                .Include(x => x.Currency) // Include related Currency entity
                .Where(x => x.DeletedAt == null); // Filter out soft-deleted records

            // Filter by organization if provided
            if (organizationID.HasValue && organizationID.Value > 0)
            {
                query = query.Where(x => x.OrganizationID == organizationID.Value);
            }

            // Sorting logic for columns
            query = sortColumn switch
            {
                "OrganizationID" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationID) : query.OrderBy(x => x.OrganizationID),
                "LanguageID" => sortOrder == "desc" ? query.OrderByDescending(x => x.LanguageID) : query.OrderBy(x => x.LanguageID),
                "TimezoneID" => sortOrder == "desc" ? query.OrderByDescending(x => x.TimezoneID) : query.OrderBy(x => x.TimezoneID),
                "CurrencyID" => sortOrder == "desc" ? query.OrderByDescending(x => x.CurrencyID) : query.OrderBy(x => x.CurrencyID),
                _ => query.OrderBy(x => x.OrganizationID)
            };

            // Use pagination service with projection
            var result = await PaginationService<Localizations, LocalizationViewModel>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%") || EF.Functions.Like(x.Currency.CurrencyName, $"%{term}%"),
                x => new LocalizationViewModel
                {
                    LocalizationID = x.LocalizationID,
                    OrganizationID = x.OrganizationID,
                    OrganizationName = x.Organization.OrganizationName ?? "_", // Assuming OrganizationName exists in Organization
                    LanguageID = x.LanguageID,
                    LanguageName = x.Language?.LanguageName ?? "_", // Assuming LanguageName exists in Language
                    TimezoneID = x.TimezoneID,
                    TimezoneName = x.Timezone?.TimezoneName ?? "_", // Assuming TimezoneName exists in Timezone
                    DateFormatID = x.DateFormatID,
                    DateFormat = x.DateFormat?.Description ?? "_", // Assuming Description exists in DateFormat
                    TimeFormatID = x.TimeFormatID,
                    TimeFormat = x.TimeFormat?.DisplayText ?? "_", // Assuming DisplayText exists in TimeFormat
                    CurrencyID = x.CurrencyID,
                    CurrencyName = x.Currency?.CurrencyName ?? "_", // Assuming CurrencyName exists in Currency
                    //CurrencySymbol = x.Currency?.CurrencySymbol ?? "_", // Assuming CurrencySymbol exists in Currency
                    //CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    //UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy,
                  //  DeletedAt = x.DeletedAt,
                    LIP = x.LIP,
                    LMAC = x.LMAC
                });

            return result;
        }
        #endregion


        #region Timezone , DateFormat, TimeFormat, Currency
        public async Task<Localizations> GetForOrganizationAsync(int orgId)
        {
            // Fetch all active (not soft-deleted) localization rows for this org
            var rows = await _genericRepository.FindAsync(x =>
                x.OrganizationID == orgId &&
                x.DeletedAt == null
            );

            // Pick the most recently updated/created one
            var loc = rows
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .FirstOrDefault();

            if (loc == null)
            {
                // No active localization configured for this organization.
                // You can replace with a default/fallback if you prefer.
                throw new InvalidOperationException(
                    $"No active localization found for OrganizationID={orgId}. " +
                    "Please create a localization (timezone/date/time format) first.");
            }

            return loc;
        }
        public async Task<string> GetIanaTimeZoneByIdAsync(int? timezoneId)
        {
            if (!timezoneId.HasValue)
                return "UTC"; // default fallback

            var tz = _genericRepositoryTimeZone.AllActive().Where(x => x.TimezoneID == timezoneId.Value).Select(x => x.TimezoneValue).FirstOrDefault();

            if (tz == null || string.IsNullOrWhiteSpace(tz))
                return "UTC"; // fallback if not found or empty

            return tz; // e.g., "Asia/Dhaka"
        }

        public async Task<string> GetDatePatternByIdAsync(int? dateFormatId)
        {
            if (!dateFormatId.HasValue)
                return "yyyy-MM-dd"; // default fallback

            var df = await _genericRepositoryDateFormat.AllActive().Where(x => x.DateFormatID == dateFormatId.Value).Select(x => x.FormatCode).FirstOrDefaultAsync();

            if (df == null || string.IsNullOrWhiteSpace(df))
                return "yyyy-MM-dd";

            return df; // e.g., "dd/MM/yyyy"
        }

        public async Task<string> GetTimePatternByIdAsync(int? timeFormatId)
        {
            if (!timeFormatId.HasValue)
                return "HH:mm"; // default fallback

            var tf = await _genericRepositoryTimeformat.AllActive().Where(x => x.TimeFormatID == timeFormatId.Value).Select(x => x.FormatCode).FirstOrDefaultAsync();

            if (tf == null || string.IsNullOrWhiteSpace(tf))
                return "HH:mm";

            return tf; // e.g., "hh:mm tt"
        }

        public async Task<OrgLocBundle> GetOrgLocalizationBundleAsync(int orgId)
        {
            var loc = await GetForOrganizationAsync(orgId);

            var tz = await GetIanaTimeZoneByIdAsync(loc.TimezoneID);
            var dp = await GetDatePatternByIdAsync(loc.DateFormatID);
            var tp = await GetTimePatternByIdAsync(loc.TimeFormatID);

            return new OrgLocBundle
            {
                TzValueOrIana = string.IsNullOrWhiteSpace(tz) ? "UTC+00:00" : tz.Trim(),
                DatePattern = string.IsNullOrWhiteSpace(dp) ? "yyyy-MM-dd" : dp.Trim(),
                TimePattern = string.IsNullOrWhiteSpace(tp) ? "HH:mm" : tp.Trim()
            };
        }
        #endregion



        #region dropdown
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
        public async Task<List<SelectListItem>> GetLanguagesAsync()
        {
            var languages = await _genericRepositoryLanguageLists.AllActive()
                .Select(l => new SelectListItem
                {
                    Value = l.ID.ToString(),
                    Text = l.LanguageName
                })
                .ToListAsync();
            
            return languages;
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
                    Text = $"{c.CurrencyName} ({c.Symbol})"

,
                })
                .ToListAsync();
            // Set first item as selected by default if any exist
            if (currencies.Any())
            {
                currencies[0].Selected = true;
            }
            return currencies;
        }
        #endregion
    }
}
