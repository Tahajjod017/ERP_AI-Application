using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeNavigtion;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeNavigation
{
    public class EmployeeNavigationService : IEmployeeNavigationService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;

        private readonly IGenericRepository<MenuTab> _menuTabRepository;

        public EmployeeNavigationService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManagerRepository2, RoleManager<ApplicationRole> roleManagerRepository2, IGenericRepository<RoleModulePermissions> rolePermissionRepository, IGenericRepository<MenuTab> menuTabRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userManagerRepository2 = userManagerRepository2;
            _roleManagerRepository2 = roleManagerRepository2;
            _rolePermissionRepository = rolePermissionRepository;
            _menuTabRepository = menuTabRepository;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                var roles = await _userManager.GetRolesAsync(user);
                return roles.ToList();
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<(string UserId, List<string> Roles)> GetCurrentUserRolesAsync(HttpContext httpContext)
        {
            try
            {
                var user = await _userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    throw new Exception("No user is currently logged in.");
                }

                var roles = await _userManager.GetRolesAsync(user);
                return (user.Id, roles.ToList());
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching user roles: " + ex.Message);
            }
        }


        public async Task<EmployeeNavigationViewModel> GetEmployeeNavigation(HttpContext httpContext, string activeTab = "", string activeSubTab = "")
        {
            var (userId, roles) = await GetCurrentUserRolesAsync(httpContext);

            // Use userId and roles as needed
            var navigationViewModel = new EmployeeNavigationViewModel
            {
                
            };

            return navigationViewModel;
        }

        // You can load this from database or configuration
        public async Task< EmployeeNavigationViewModel> GetEmployeeNavigation(string activeTab = "", string activeSubTab = "")
        {

           

            var userId = "52add4b8-af22-49c1-b8ea-e838b9f6ef7e";

            var user = await _userManagerRepository2.FindByIdAsync(userId); // Get the ApplicationUser
            var roleNames = await _userManagerRepository2.GetRolesAsync(user); // List<string> of role names

            //var roleIds = _roleManagerRepository2.Roles
            //             .Where(role => roleNames.Contains(role.Name))
            //             .Select(role => role.Id)
            //             .ToList();

            //var menuTabs = (from rp in _rolePermissionRepository.All()
            //                join mt in _menuTabRepository.All() on rp.MenuTabId equals mt.MenuTabId
            //                where roleIds.Contains(rp.RoleId) && mt.ControllerName.StartsWith("Employee")
            //                select mt)
            //     .Distinct()
            //     .ToList();


            var navigation = new EmployeeNavigationViewModel
            {
                ActiveTab = activeTab,
                ActiveSubTab = activeSubTab
            };

            // Main navigation items
            navigation.MainNavigation = new List<NavigationItem>
            {
                new NavigationItem
                {
                    Id = "headingPInfo",
                    Text = "Personal Info.",
                    Url = "/EmployeePersonal/index",
                    Icon = "fa-solid fa-user",
                    IsActive = activeTab == "PersonalInfo",
                    AriaControls = "empPersonalInfo"
                },
                new NavigationItem
                {
                    Id = "headingEmpOfficialInfo",
                    Text = "Official Info.",
                    Url = "/EmployeeOfficial/index",
                    Icon = "fa-solid fa-file-alt",
                    IsActive = activeTab == "OfficialInfo",
                    AriaControls = "empOfficialInfo"
                },
                new NavigationItem
                {
                    Id = "headingempPayroleInfo",
                    Text = "Payroll Info.",
                    Url = "/EmployeeSalary/index",
                    Icon = "fa-solid fa-file-alt",
                    IsActive = activeTab == "PayrollInfo",
                    AriaControls = "empPayroleInfo"
                },
                new NavigationItem
                {
                    Id = "headingAdditionalInfo",
                    Text = "Additional Info.",
                    Url = "/EmployeeAdditional/index",
                    Icon = "fa-solid fa-square-check",
                    IsActive = activeTab == "AdditionalInfo",
                    AriaControls = "additionalInfo"
                },
                new NavigationItem
                {
                    Id = "headingeducationInfo",
                    Text = "Education Info.",
                    Url = "/EmployeeEducation/index",
                    Icon = "fa-solid fa-square-check",
                    IsActive = activeTab == "EducationInfo",
                    AriaControls = "educationInfo"
                },
                new NavigationItem
                {
                    Id = "headingTranningInfo",
                    Text = "Training Info.",
                    Url = "/EmployeeTraining/index",
                    Icon = "fa-solid fa-square-check",
                    IsActive = activeTab == "TrainingInfo",
                    AriaControls = "tranningInfo"
                },
                new NavigationItem
                {
                    Id = "headingFamilyInfo",
                    Text = "Family Info.",
                    Url = "/EmployeeFamily/index",
                    Icon = "fa-solid fa-square-check",
                    IsActive = activeTab == "FamilyInfo",
                    AriaControls = "familyInfo"
                },
                new NavigationItem
                {
                    Id = "headingemmerhencyContactInfo",
                    Text = "Emergency Contact",
                    Url = "/EmployeeContact/index",
                    Icon = "fa-solid fa-square-check",
                    IsActive = activeTab == "EmergencyContact",
                    AriaControls = "emmerhencyContactInfo"
                }
            };

            // Sub navigation for Payroll (only when PayrollInfo is active)
            if (activeTab == "PayrollInfo")
            {
                navigation.SubNavigation = new List<NavigationItem>
            {
                new NavigationItem
                {
                    Id = "empSalary-tab",
                    Text = "Employee Salary Settings",
                    Url = "/EmployeeSalary/index",
                    IsActive = activeSubTab == "EmployeeSalary",
                    AriaControls = "tab-empSalary"
                },
                new NavigationItem
                {
                    Id = "empBenefits-tab",
                    Text = "Employee Benefits",
                    Url = "/EmployeeBenefits/index",
                    IsActive = activeSubTab == "EmployeeBenefits",
                    AriaControls = "tab-empBenefits"
                },
                new NavigationItem
                {
                    Id = "empAllowance-tab",
                    Text = "Employee Allowance",
                    Url = "/EmployeeAllowance/index",
                    IsActive = activeSubTab == "EmployeeAllowance",
                    AriaControls = "tab-empAllowance"
                }
            };
            }

            return navigation;
        }

        // Alternative: Load from database
        public EmployeeNavigationViewModel GetEmployeeNavigationFromDatabase(string userId, string activeTab = "", string activeSubTab = "")
        {
            // Load navigation items from database based on user permissions
            // This method would query your database for navigation items
            // and filter based on user permissions

            var navigation = new EmployeeNavigationViewModel
            {
                ActiveTab = activeTab,
                ActiveSubTab = activeSubTab
            };

            // Example database query (pseudo-code)
            // var navItems = _dbContext.NavigationItems
            //     .Where(n => n.IsActive && UserHasPermission(userId, n.Permission))
            //     .OrderBy(n => n.Order)
            //     .ToList();

            // navigation.MainNavigation = navItems.Select(n => new NavigationItem { ... }).ToList();

            return navigation;
        }
    }
}
