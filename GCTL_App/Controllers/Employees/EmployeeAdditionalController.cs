
﻿using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeAdditional;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeAdditional;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;

using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeAdditionalController : BaseController
    {
        #region CTOR
        private readonly IEmployeeAdditionalService _employeeAdditionalService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.LicenceTypes> _licenceTypesRepository;

        private readonly IEmployeeNavigationService _employeeNavigationService;


    
        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;

        public EmployeeAdditionalController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeAdditionalService employeeAdditionalService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<LicenceTypes> licenceTypesRepository, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2) : base(translateService, userProfileService)
        {
            _employeeAdditionalService = employeeAdditionalService;
            _employeeRepository = employeeRepository;
            _licenceTypesRepository = licenceTypesRepository;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository2 = userManagerRepository2;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
        }
        #endregion


        public async Task<IActionResult> Index(int id)
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


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "AdditionalInfo");
                ViewBag.Navigation = navigationModel;
            }


            

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
           
           
            ViewBag.LicenseTypeDD = _licenceTypesRepository.GetSelectListById( e => e.LicenceTypeID, e => e.LicenceTypeName);

            var employee =  _employeeAdditionalService.GetEmployeeAdditionalByIdAsync(id).Result;


            SetSmartPageCode(113000);
            return View(employee);
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmployeeAdditionalPostViewModel model)
        {
            var res = await _employeeAdditionalService.SubmitAsync(model);
            return Ok(res);

        }

        [HttpPost]
        public async Task<IActionResult> SubmitFromEdit(EmployeeAdditionalPostViewModel model)
        {
            //return Ok(model);

            var res = await _employeeAdditionalService.SubmitAsync(model);
            return Ok(res);

        }

        public async Task<IActionResult> GetEmployeeData(int id)
        {

            var employee = await _employeeAdditionalService.GetEmployeeAdditionalByIdAsync(id);

            return Ok(employee);
        }
    }
}
