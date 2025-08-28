using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Data.Models;
using GCTL.Service.CRM;
using GCTL.Service.Employees.EmployeeResign;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.CRM
{
    public class CRMController : BaseController
    {
        private readonly ICRMService _crmService;
        private readonly IGenericRepository<AddressTypes> _addressTypeService;
        public CRMController(IGenericRepository<AddressTypes> addressTypeService, ITranslateService translateService, IUserProfileService userProfileService, ICRMService crmService) : base(translateService, userProfileService)
        {
            _crmService = crmService;
            _addressTypeService = addressTypeService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(605000);

            ViewBag.ServiceTypeDD = new SelectList(_addressTypeService.AllActive().Where(u => u.AddressTypeName == "billing" || u.AddressTypeName == "company").Select(e => new { e.AddressTypeID, e.AddressTypeName }), "AddressTypeID", "AddressTypeName");
            return View();
        }

        #region Approved

        [HttpGet]
        public async Task<IActionResult> GetAllLead(
     string dateRange,
     int customerType,
     string designation,
     int pageNumber = 1,
     int pageSize = 10,
     string searchTerm = "",
     string sortColumn = "",
     string sortDirection = "desc")
        {
            var (leads, totalCount) = await _crmService.GetLeads(
                customerType, dateRange, pageNumber, pageSize,
                searchTerm, sortColumn, sortDirection
            );

            var result = new LeadListViewModel
            {
                Leads = leads,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            return Json(new { result });
        }

        #endregion
    }



}
