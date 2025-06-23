using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeePersonalController : BaseController
    {
        #region CTOR
        private readonly IEmployeePersonalService _employeePersonalService;
        private readonly IEmployeeNavigationService _employeeNavigationService;
        private readonly IGenericRepository<MaritalStatus> _maritalRepository;
        private readonly IGenericRepository<Religions> _religionRepository;
        private readonly IGenericRepository<Genders> _genderRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<BloodGroup> _bloodGroupRepository;

        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;

        private readonly IElementPermissionService _elementPermissionService;
        public EmployeePersonalController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeePersonalService employeePersonalService, IGenericRepository<MaritalStatus> maritalRepository, IGenericRepository<Religions> religionRepository, IGenericRepository<Genders> genderRepository, IGenericRepository<Country> countryRepository, IGenericRepository<BloodGroup> bloodGroupRepository, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2, IElementPermissionService elementPermissionService) : base(translateService, userProfileService)
        {
            _employeePersonalService = employeePersonalService;
            _maritalRepository = maritalRepository;
            _religionRepository = religionRepository;
            _genderRepository = genderRepository;
            _countryRepository = countryRepository;
            _bloodGroupRepository = bloodGroupRepository;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository2 = userManagerRepository2;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
            _elementPermissionService = elementPermissionService;
        }

        #endregion

        public async Task<IActionResult> Index()
        {
            SetSmartPageCode(111000);

            PopulateViewBag();

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


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "PersonalInfo");
                ViewBag.Navigation = navigationModel;
            }

            

            //ViewBag.MaritalStatusDD = _maritalRepository.All().ToList();
            //EmployeePersonalPostViewModel model = null;
            //if (TempData["EmployeeModel"] != null)
            //{
            //    model = JsonConvert.DeserializeObject<EmployeePersonalPostViewModel>(TempData["EmployeeModel"].ToString());
            //}
            //return View(model);

            return View();
            
        }

        private void PopulateViewBag()
        {
            ViewBag.MaritalStatusDD = new SelectList(_maritalRepository.All(), "MaritalStatusID", "MaritalStatusName");
            ViewBag.ReligionDD = new SelectList(_religionRepository.All(), "ReligionID", "ReligionName");
            ViewBag.GenderDD = new SelectList(_genderRepository.All(), "GenderID", "GenderName");
            ViewBag.BloodGroupDD = new SelectList(_bloodGroupRepository.All(), "BloodGroupID", "BloodGroupName");
            ViewBag.CountryDD = new SelectList(_countryRepository.All(), "CountryID", "CountryName");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmployeePersonalPostViewModel model)
        {
            PopulateViewBag();

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


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "PersonalInfo");
                ViewBag.Navigation = navigationModel;
            }



            if (ModelState.IsValid)
            {
                var chkDuplicate = await _employeePersonalService.CheckValidEmployeeInfo(model);

                if (!chkDuplicate.Success)
                {
                    TempData["ToastrMessage"] = chkDuplicate.Message;
                    TempData["ToastrType"] = "warning";

                 
                    return View(model); 
                }

                // Save and get the new employee ID
                CommonReturnViewModel result = await _employeePersonalService.SaveEmployeePersonalInfo(model);

                if (result.Success)
                {
                    TempData["ToastrMessage"] = "Employee saved successfully!";
                    TempData["ToastrType"] = "success";
                    return RedirectToAction("Index", "EmployeeOfficial", new { id = result.Data });
                }
                else
                {
                    TempData["ToastrMessage"] = result.Message;
                    TempData["ToastrType"] = "error";


                    return View(model);
                }

                
            }

            //TempData["EmployeeModel"] = JsonConvert.SerializeObject(model); 
            //return RedirectToAction(nameof(Index));
          
            return View(model);
        }


        [HttpPost]

        public async Task<IActionResult> SubmitFromEdit(EmployeePersonalPostViewModel model)
        {

            var chkDuplicate = await _employeePersonalService.CheckValidEmployeeInfo(model);

            if (!chkDuplicate.Success)
            {
                return Ok(chkDuplicate);
            }

            // Save and get the new employee ID
            CommonReturnViewModel result = await _employeePersonalService.SaveEmployeePersonalInfo(model);

            return Ok(result);


        }

        #region Nationality

        [HttpGet]
        public IActionResult GetNationalities()
        {
            var nationalities = _countryRepository.All()
                .OrderBy(n => n.CountryName)
                .Select(n => n.CountryName)
                .ToList();

            return Json(nationalities);
        }


        [HttpPost]
        public async Task< IActionResult> SaveNationality([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Nationality name is required.");

            // Example: Save to database (pseudo code)
            var exists = _countryRepository.All().Any(n => n.CountryName == name);
            if (!exists)
            {
                var nationality = new Country { CountryName = name };

                await _countryRepository.AddAsync(nationality);
                
            }

            return Ok(new { success = true, name });
        }

        #endregion

        #region Role Element Permission for Tab

        [HttpGet]
        public async Task<IActionResult> RoleElementPermission()
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

                // Check if the user has permission to choose an employee (for a specific page and element)
                bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeDropDown");

                
                

                return Ok(hasEmployeePermission);
            }

            return Ok(false);

        }

        #endregion


    }
}
