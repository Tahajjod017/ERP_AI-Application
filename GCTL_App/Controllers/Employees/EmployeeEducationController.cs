

using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;


using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol.Core.Types;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeEducationController : BaseController
    {
        #region CTOR
        private readonly IEmployeeEducationalService _employeeEducationalService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EducationLevels> _educationLevelsRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Degree> _degreeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EducationBoard> _educationBoardRepository;
        private readonly IGenericRepository<GCTL.Data.Models.ResultTypes> _resultTypeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.PassingYears> _passingYearRepository;

        private readonly IEmployeeNavigationService _employeeNavigationService;

        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;

        public EmployeeEducationController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeEducationalService employeeEducationalService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<GCTL.Data.Models.EducationLevels> educationLevelsRepository, IGenericRepository<GCTL.Data.Models.Degree> degreeRepository, IGenericRepository<GCTL.Data.Models.EducationBoard> educationBoardRepository, IGenericRepository<GCTL.Data.Models.ResultTypes> resultTypeRepository, IGenericRepository<GCTL.Data.Models.PassingYears> passingYearRepository, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2) : base(translateService, userProfileService)
        {
            _employeeEducationalService = employeeEducationalService;
            _employeeRepository = employeeRepository;
            _educationLevelsRepository = educationLevelsRepository;
            _degreeRepository = degreeRepository;
            _educationBoardRepository = educationBoardRepository;
            _resultTypeRepository = resultTypeRepository;
            _passingYearRepository = passingYearRepository;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository2 = userManagerRepository2;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
        }
        #endregion

        public async Task< IActionResult> Index(int id)
        {

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


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "EducationInfo");
                ViewBag.Navigation = navigationModel;
            }

            


            var eduList = _employeeEducationalService.GetEmployeeAdditionalByIdAsync(id).Result;

            ViewBag.EduList = eduList;

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");

            ViewBag.EducationLevel = _educationLevelsRepository.GetActiveSelectListById(e => e.EducationLevelID, e => e.EducationLevelName);
            ViewBag.Degree = _degreeRepository.GetActiveSelectListById(e => e.DegreeID, e => e.DegreeName);
            ViewBag.EducationBoard = _educationBoardRepository.GetActiveSelectListById(e => e.EducationBoardID, e => e.EducationBoardName);
            ViewBag.ResultType = _resultTypeRepository.GetActiveSelectListById(e => e.ResultTypeID, e => e.ResultTypeName);
            ViewBag.PassingYear = _passingYearRepository.GetActiveSelectListById(e => e.PassingYearID, e => e.PassingYearName);

            SetSmartPageCode(114000);
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmployeeEducationalPostViewModel model)
        {
            var res = await _employeeEducationalService.SaveAsync(model);
            return Ok(res);

        }



        [HttpPost]
        public async Task<IActionResult> SubmitFromEdit(EmployeeEducationalPostViewModel model)
        {
            var a = GetEmployeePictureURL();
            var res = await _employeeEducationalService.SaveAsync(model);
            var data = await _employeeEducationalService.GetEmployeeAdditionalByIdAsync(model.EmployeePersonalId);
            res.Data = data;
            return Ok(res);

        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            var res = await _employeeEducationalService.DeleteAsync(id);
            return Ok(res);

        } 
        
        [HttpPost]
        public async Task<IActionResult> DeleteFromEdit(int id)
        {

            var res = await _employeeEducationalService.DeleteAsync(id);
            if (res.Success)
            {
                var data = await _employeeEducationalService.GetEmployeeAdditionalByIdAsync(Convert.ToInt32(res.Data));
                res.Data = data;

            }
            return Ok(res);

        }

        //Return List

        [HttpGet]
        public async Task<IActionResult> GetEmployeeData(int id)
        {

            var employee = await _employeeEducationalService.GetEmployeeAdditionalByIdAsync(id);

            return Ok(employee);
        }

        //Return Single

        [HttpGet]
        public async Task< IActionResult> GetEmployeeEduData(int id)
        {
            try
            {
                var data =  await _employeeEducationalService.GetEmployeeEduData(id);
                return Json(data);
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Update([FromBody] EmployeeEducationalPostViewModel model)
        {
            if (model == null || model.EmployeeEducationalInfoID <= 0)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var res = await _employeeEducationalService.UpdateAsync(model);
            return Ok(res);
        }

    }
}
