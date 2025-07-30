using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeStatus.Promotion;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.PromotionController
{
    public class PromotionListController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOffiRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<EmployeeCareerChanges> _empCarrerRepository;
        private readonly IPromotionService _promotionService;

        public PromotionListController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmployeeCareerChanges> empCarrerRepository, IPromotionService promotionService) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeOffiRepository = employeeOffiRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _empCarrerRepository = empCarrerRepository;
            _promotionService = promotionService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(112100);


            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new
            {
                id = e.EmployeeID,
                name = e.FirstName + " " + e.LastName
            }), "id", "name");

            ViewBag.OrganizationDD = new SelectList(
               _organizationRepository.All().Select(o => new { o.OrganizationID, o.OrganizationName }),
               "OrganizationID",
               "OrganizationName"
           );



            ViewBag.DepartmentDD = new SelectList(
                _departmentRepository.All().Select(d => new { d.DepartmentID, d.DepartmentName }),
                "DepartmentID",
                "DepartmentName"
            );

            ViewBag.DesignationDD = new SelectList(
                _designationRepository.All().Select(d => new { d.DesignationID, d.DesignationName }),
                "DesignationID",
                "DesignationName"
            );

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GetPromotionList(PromotionListFilterViewModel filters)
        {
            //var result = await _promotionService.GetPagedPromotionListAsync(filters);

            //return Json(new
            //{
            //    success = true,
            //    data = new
            //    {
            //        items = result.Items,
            //        pagination = new
            //        {
            //            currentPage = result.CurrentPage,
            //            totalPages = result.TotalPages,
            //            startRecord = result.StartRecord,
            //            endRecord = result.EndRecord,
            //            totalRecords = result.TotalRecords
            //        }
            //    }
            //});


            return Json(new { success = true, });


        }

    }
}
