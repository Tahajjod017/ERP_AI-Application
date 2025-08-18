using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Data.Models;
using GCTL.Service.CRM;
using GCTL.Service.Employees.EmployeeResign;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.CRM
{
    public class CRMController : BaseController
    {
        private readonly ICRMService _crmService;
        public CRMController(ITranslateService translateService, IUserProfileService userProfileService, ICRMService crmService) : base(translateService, userProfileService)
        {
            _crmService = crmService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(600000);
            return View();
        }

        #region Approved

        [HttpGet]
        public async Task<IActionResult> GetAllLead(
            string dateRange,
            string customerType,
            string designation,
            int pageNumber = 1,
            int pageSize = 10,
            string searchTerm = "",
            string sortColumn = "",
            string sortDirection = "desc")
        {
            // Fetch all leads
            var leadList = await _crmService.GetLeads(customerType);

            // Apply search safely
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                leadList = leadList.Where(r =>
                    (r.LeadName?.ToLower().Contains(searchTerm) ?? false) ||
                    (r.Phone?.ToLower().Contains(searchTerm) ?? false) ||
                    (r.ContactName?.ToLower().Contains(searchTerm) ?? false) ||
                    (r.CompanyName?.ToLower().Contains(searchTerm) ?? false)
                ).ToList();
            }

            // Apply sorting safely
            if (!string.IsNullOrEmpty(sortColumn))
            {
                leadList = sortColumn switch
                {
                    "leadName" => sortDirection == "desc" ? leadList.OrderBy(r => r.LeadName ?? "").ToList() : leadList.OrderByDescending(r => r.LeadName ?? "").ToList(),
                    "email" => sortDirection == "desc" ? leadList.OrderBy(r => r.Email ?? "").ToList() : leadList.OrderByDescending(r => r.Email ?? "").ToList(),
                    "phone" => sortDirection == "desc" ? leadList.OrderBy(r => r.Phone ?? "").ToList() : leadList.OrderByDescending(r => r.Phone ?? "").ToList(),
                    "contactName" => sortDirection == "desc" ? leadList.OrderBy(r => r.ContactName ?? "").ToList() : leadList.OrderByDescending(r => r.ContactName ?? "").ToList(),
                    "companyName" => sortDirection == "desc" ? leadList.OrderBy(r => r.CompanyName ?? "").ToList() : leadList.OrderByDescending(r => r.CompanyName ?? "").ToList(),
                    "status" => sortDirection == "desc" ? leadList.OrderBy(r => r.Status ?? "").ToList() : leadList.OrderByDescending(r => r.Status ?? "").ToList(),
                    _ => leadList
                };
            }

            // Total count before paging
            var totalCount = leadList.Count;

            // Apply paging
            var pagedLeadList = leadList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Prepare result
            var result = new LeadListViewModel
            {
                Leads = pagedLeadList,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            // Return JSON
            return Json(new { result });
        }

        #endregion
    }



}
