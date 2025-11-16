using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.CRM.LeadCreate;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace GCTL_App.Controllers.CRM
{
    [Authorize]
    public class CreateLeadController : BaseController
    {
        #region Repositories & Services
        private readonly ILeadCreateService _leadCreateService;

        public CreateLeadController(AppDbContext context, ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Services> serviceTypeRepository, IGenericRepository<LeadSources> leadSourceTypeRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<CustomerAddresses> customerAddressesRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeTypeRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<LeadStatuses> leadStatusesTypeRepository = null, ILeadCreateService leadCreateService = null, IGenericRepository<Priorities> prioritiesRepository = null) : base(translateService, userProfileService)
        {
            _leadCreateService = leadCreateService;
        }
        #endregion

        #region Index
        public IActionResult index()
        {
            SetSmartPageCode(600100);
            return View();
        }
        #endregion

        #region CreateLeadData
        [Permission("Create", "CreateLead")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateLeadData([FromBody] LeadsVM leadsVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _leadCreateService.CreateLead(await GetCurrentOrganizationIdAsync() ?? 0, leadsVM);
                return Ok(result);
            }

            return Ok(new ReturnView
            {
                Success = false,
                Message = "Data not inserted"
            });
        }
        #endregion

        #region Get LeadOwner List
        [HttpGet]
        public async Task<IActionResult> GetLeadOwnerList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetLeadOwnerListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.LeadID,
                    label = $"{c.LeadName} {c.Phone} {c.Email}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion

        #region Get LeadSource List
        [HttpGet]
        public async Task<IActionResult> GetLeadSourceList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetLeadSourceListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Id,
                    label = $"{c.Name}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion

        #region Get LeadStatus List
        [HttpGet]
        public async Task<IActionResult> GetLeadStatusList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetLeadStatusListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Id,
                    label = $"{c.Name}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion

        #region Get GetPriorityListAsync List
        [HttpGet]
        public async Task<IActionResult> GetPriorityList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetPriorityListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Id.ToString(),
                    label = $"{c.Name ?? ""}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion

        #region Get GetPriorityListAsync List
        [HttpGet]
        public async Task<IActionResult> GetServiceList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetServiceListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Id.ToString(),
                    label = $"{c.Name ?? ""}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion
    }
}
