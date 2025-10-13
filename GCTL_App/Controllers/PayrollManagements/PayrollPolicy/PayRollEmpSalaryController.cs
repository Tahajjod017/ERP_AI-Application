
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.LipLmacAddress;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollEmpAllowance;
using GCTL.Service.PayRollManagements.PayRollEmpSalary;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    [Authorize]
    public class PayRollEmpSalaryController : BaseController
    {
        private IPayRollEmpSalaryService payRollEmpSalaryService;
        private readonly IGenericRepository<Organization> organization;
        private readonly IGenericRepository<Departments> departmentsRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository;
        public PayRollEmpSalaryController(ITranslateService translateService, IUserProfileService userProfileService, IPayRollEmpSalaryService payRollEmpSalaryService, IGenericRepository<Organization> organization, IGenericRepository<Departments> departmentsRepository, IGenericRepository<GCTL.Data.Models.Employees> employee, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository) : base(translateService, userProfileService)
        {
            this.payRollEmpSalaryService = payRollEmpSalaryService;
            this.organization = organization;
            this.departmentsRepository = departmentsRepository;
            this.employee = employee;
            this.employeeOfficeInfoRepository = employeeOfficeInfoRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var orgList = organization.AllActive()
                    .Select(x => new { x.OrganizationID, x.OrganizationName })
                    .ToList();

                var deptList = departmentsRepository.AllActive()
                    .Select(x => new { x.DepartmentID, x.DepartmentName })
                    .ToList();

                var empList = employee.AllActive()
                    .Select(x => new { x.EmployeeID, x.FirstName, x.LastName })
                    .ToList();

                // Organization dropdown
                ViewBag.OrganizationDD = new SelectList(
                    orgList,
                    "OrganizationID",
                    "OrganizationName",
                    orgList.Count == 1 ? orgList.First().OrganizationID : (int?)null
                );

                // Department dropdown
                ViewBag.DepartmentDD = new SelectList(
                    deptList,
                    "DepartmentID",
                    "DepartmentName",   // ✅ Fixed typo
                    deptList.Count == 1 ? deptList.First().DepartmentID : (int?)null
                );

                

                var a = (from e in employee.AllActive().AsQueryable()
                         join emoff in employeeOfficeInfoRepository.AllActive().AsQueryable()
                             on e.EmployeeID equals emoff.EmployeeID
                         join d in departmentsRepository.AllActive().AsQueryable()
                             on emoff.DepartmentID equals d.DepartmentID
                         select new EmployeeGroupVM
                         {
                             EmployeeID = e.EmployeeID,
                             FullName =$"{e.FirstName} {e.LastName}",
                             DepartmentName = d.DepartmentName
                         }).ToList();

                ViewBag.GroupedEmployees = a .GroupBy(e => e.DepartmentName) .ToDictionary(g => g.Key, g => g.ToList());




            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }


            return View();
        }
        

        #region Get All Data List

        [Route("PayRollEmpSalary/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null, List<int>? deptID = null, List<int>? empID = null)
        {
            try
            {

                var data = await payRollEmpSalaryService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, organizationId, imgSrcThumb,deptID, empID);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        #endregion


        #region save pdf 

        [HttpPost]
        [Route("PaySlipForEmp/GenerateMultiplePDFs")]
       
        public async Task<IActionResult> GenerateMultiplePDFs([FromBody] PayRollEmpSalarySaveVM model)
        {
            try
            {
                var saveResult = await payRollEmpSalaryService.SaveExportAsync(model);

                if (!saveResult.Success || saveResult.Data == null)
                    return Json(new { Success = false, Message = saveResult.Message });
                return Json(new { Success = true, Message = saveResult.Message });
            }
            catch (Exception)
            {

                throw;
            }
            // 1️⃣ Save payslip
           
        }

        #endregion
    }
}
