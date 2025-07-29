
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Core.ViewModels.Employee.EmployeeTraining;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Employees.EmployeeTraining;
using GCTL.Service.Language;


using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeTrainingController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<TrainingYears> _trainingYearsRepository;

        private readonly IEmployeeTrainingService _employeeTrainingService;

        private readonly IEmployeeNavigationService _employeeNavigationService;

        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;
        private readonly IElementPermissionService _elementPermissionService;

        public EmployeeTrainingController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeTrainingService employeeTrainingService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Country> countryRepository, IGenericRepository<TrainingYears> trainingYearsRepository, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2, IElementPermissionService elementPermissionService) : base(translateService, userProfileService)
        {
            _employeeTrainingService = employeeTrainingService;
            _employeeRepository = employeeRepository;
            _countryRepository = countryRepository;
            _trainingYearsRepository = trainingYearsRepository;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository2 = userManagerRepository2;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
            _elementPermissionService = elementPermissionService;
        }


        public async Task< IActionResult> Index(int id)
        {
            SetSmartPageCode(115000);
            var loggedUser = await _userManagerRepository2.GetUserAsync(User);

            if (loggedUser != null)
            {

                var userId = loggedUser.Id;

                var user = await _userManagerRepository2.FindByIdAsync(userId); // Get the ApplicationUser
                var roleNames = await _userManagerRepository2.GetRolesAsync(user); // List<string> of role names

                var roleIds = _roleManagerRepository2.Roles.Where(role => roleNames.Contains(role.Name)).Select(role => role.Id).ToList();

                var menuTabs = (from rp in _rolePermissionRepository.All()
                                join mt in _menuTabRepository.All() on rp.MenuTabId equals mt.MenuTabId
                                where roleIds.Contains(rp.RoleId) && mt.ControllerName.StartsWith("Employee")
                                select mt.ControllerName).Distinct().ToList();


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "TrainingInfo");
                ViewBag.Navigation = navigationModel;
            }

            

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
            ViewBag.Country = _countryRepository.GetActiveSelectListById(c => c.CountryID, c => c.CountryName);
            ViewBag.TrainingYear = _trainingYearsRepository.GetActiveSelectListById(t => t.TrainingYearID, t => t.TrainingYearName);

            

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmployeeTrainingPostViewModel model)
        {
            var res = await _employeeTrainingService.SaveAsync(model);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFromEdit(EmployeeTrainingPostViewModel model)
        {
            var res = await _employeeTrainingService.SaveAsync(model);
            var employee = await _employeeTrainingService.GetEmployeeTrainingByIdAsync(model.EmployeePersonalId);
            res.Data = employee;
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _employeeTrainingService.DeleteAsync(id);
            return Ok(res);
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteFromEdit(int id)
        {
            var res = await _employeeTrainingService.DeleteAsync(id);
            if (res.Success)
            {
                var employee = await _employeeTrainingService.GetEmployeeTrainingByIdAsync(Convert.ToInt32(res.Data));
                res.Data = employee;
            }
            return Ok(res);
        }

        //List

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


                        var employee = await _employeeTrainingService.GetEmployeeTrainingByIdAsync((int)empid);

                        return Ok(employee);
                    }


                }
                else
                {

                    var employee = await _employeeTrainingService.GetEmployeeTrainingByIdAsync(id);

                    return Ok(employee);
                }


            }

            return Ok();
           
        }


        [HttpGet]
        public async Task<IActionResult> GetEmployeeTrainingData(int id)
        {
            try
            {
                var data = await _employeeTrainingService.GetEmployeeEduData(id);
                return Ok(data);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Update([FromBody] EmployeeTrainingPostViewModel model)
        {
            if (model == null || model.EmployeeTranningInfoID <= 0)
            {
                return Ok(new { success = false, message = "Invalid data" });
            }

            var res = await _employeeTrainingService.UpdateAsync(model);
            return Ok(res);
        }





    }
}
