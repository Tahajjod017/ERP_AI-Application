using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Settings;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Settings
{
    public class ApprovalMatrixController : BaseController
    {
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<OrganizationBranches> _organizationBranchRepository;
        private readonly IGenericRepository<ApprovalTypes> _approvalTypeRepository;
        private readonly IGenericRepository<ApprovalSettings> _approvalSettingRepository;
        
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        public ApprovalMatrixController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organizationRepository, IGenericRepository<OrganizationBranches> organizationBranchRepository, IGenericRepository<ApprovalTypes> approvalTypeRepository, IGenericRepository<ApprovalSettings> approvalSettingRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository) : base(translateService, userProfileService)
        {
            _organizationRepository = organizationRepository;
            _organizationBranchRepository = organizationBranchRepository;
            _approvalTypeRepository = approvalTypeRepository;
            _approvalSettingRepository = approvalSettingRepository;
            _employeeRepository = employeeRepository;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(995485200);

            ViewBag.Organizations = new SelectList(_organizationRepository.AllActive().Select(e => new { e.OrganizationID, e.OrganizationName }), "OrganizationID", "OrganizationName");
            ViewBag.OrganizationBranches = new SelectList(_organizationBranchRepository.AllActive().Select(e => new { e.OrganizationBranchID, e.OrganizationBranchName }), "OrganizationBranchID", "OrganizationBranchName");
            ViewBag.ApprovalTypes = new SelectList(_approvalTypeRepository.AllActive().Select(e => new { e.ApprovalTypeID, e.ApprovalTypeName }), "ApprovalTypeID", "ApprovalTypeName");
            // ViewBag.Employees = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " "+ e.LastName }), "EmployeeID", "FullName");


            ViewBag.Employees = new SelectList(_employeeRepository.AllActive()
                 .Include(e => e.EmployeeOfficeInfoEmployee)
                 .ThenInclude(r => r.Designation)
                 .OrderBy(e => e.EmployeeOfficeInfoEmployee.First().Designation.Ranking)

                 .Select(e => new
                 {
                     id = e.EmployeeID,
                     name = e.FirstName + " " + e.LastName +
                            (e.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation != null
                                ? " (" + e.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation.DesignationName + ")"
                                : "")
                 }).ToList(),
             "id", "name");



            return View();
        }




        

    }
}
