
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeFamily;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeFamilyController : BaseController
    {

        private readonly IEmployeeFamilyService _employeeFamilyService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IEmployeeNavigationService _employeeNavigationService;

        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;
        private readonly IElementPermissionService _elementPermissionService;

        public EmployeeFamilyController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeFamilyService employeeFamilyService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2, IElementPermissionService elementPermissionService) : base(translateService, userProfileService)

        {
            _employeeFamilyService = employeeFamilyService;
            _employeeRepository = employeeRepository;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository2 = userManagerRepository2;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
            _elementPermissionService = elementPermissionService;
        }

        public async Task< IActionResult> Index(int id)
        {
            var loggedUser = await _userManagerRepository2.GetUserAsync(User);

            if (loggedUser != null)
            {

                var userId = loggedUser.Id;

                var user = await _userManagerRepository2.FindByIdAsync(userId); // Get the ApplicationUser
                var roleNames = await _userManagerRepository2.GetRolesAsync(user); // List<string> of role names

                var roleIds = _roleManagerRepository2.Roles.Where(role => roleNames.Contains(role.Name)).Select(role => role.Id).ToList();

                var menuTabs = (from rp in _rolePermissionRepository.AllActive()
                                join mt in _menuTabRepository.AllActive() on rp.MenuTabId equals mt.MenuTabId
                                where roleIds.Contains(rp.RoleId) && mt.ControllerName.StartsWith("Employee")
                                select mt.ControllerName).Distinct().ToList();


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "FamilyInfo");
                ViewBag.Navigation = navigationModel;
            }

           

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");


            SetSmartPageCode(116000);
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmployeeFamilyPostViewModel model)
        {
            var res = await _employeeFamilyService.SaveAsync(model);
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> SubmitFromEdit(EmployeeFamilyPostViewModel model)
        {
            var res = await _employeeFamilyService.SaveAsync(model);
            var employee = await _employeeFamilyService.GetEmployeeFamilyByIdAsync(model.EmployeePersonalId);
            res.Data = employee;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _employeeFamilyService.DeleteAsync(id);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFromEdit(int id)
        {
            var res = await _employeeFamilyService.DeleteAsync(id);
            if (res.Success)
            {
                var employee = await _employeeFamilyService.GetEmployeeFamilyByIdAsync(Convert.ToInt32(res.Data));
                res.Data = employee;
            }
            return Ok(res);
        }

        //list

        [HttpGet]
        public async Task<IActionResult> GetEmployeeData(int id)
        {

            var loggedUser = await _userManagerRepository2.GetUserAsync(User);

            if (loggedUser != null)
            {
                var userId = loggedUser.Id;
                bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeTable");

                if (!hasEmployeePermission)
                {
                    var empid = loggedUser.EmployeeId;

                    if (empid == null || empid == 0)
                    {
                        return Ok();

                    }
                    else
                    {


                        var employee = await _employeeFamilyService.GetEmployeeFamilyByIdAsync((int)empid);
                        return Ok(employee);
                    }


                }
                else
                {

                    var employee = await _employeeFamilyService.GetEmployeeFamilyByIdAsync(id);
                    return Ok(employee);
                }


            }

            return Ok();

           
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeFamilyData(int id)
        {
            try
            {
                var loggedUser = await _userManagerRepository2.GetUserAsync(User);

                if (loggedUser != null)
                {
                    var userId = loggedUser.Id;
                    bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeTable");

                    if (!hasEmployeePermission)
                    {
                        var empid = loggedUser.EmployeeId;

                        if (empid == null || empid == 0)
                        {
                            return Ok();

                        }
                        else
                        {


                            var data = await _employeeFamilyService.GetEmployeeFamilyData((int)empid);
                            return Ok(data);
                        }


                    }
                    else
                    {

                        var data = await _employeeFamilyService.GetEmployeeFamilyData(id);
                        return Ok(data);
                    }


                }

                return Ok();

               
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] EmployeeFamilyPostViewModel model)
        {
            if (model == null || model.EmployeeFamilyInfoID <= 0)
            {
                return Ok(new { success = false, message = "Invalid data" });
            }

            var res = await _employeeFamilyService.UpdateAsync(model);
            return Ok(res);
        }

    }
}
