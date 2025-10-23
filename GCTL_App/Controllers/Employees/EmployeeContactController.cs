using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeContact;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeContactController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeFamilyInfo> _employeeFamilyInfoRepository;

        private readonly IEmployeeContactService _employeeContactService;

        private readonly IEmployeeNavigationService _employeeNavigationService;

        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;

        private readonly IElementPermissionService _elementPermissionService;


        public EmployeeContactController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IEmployeeContactService employeeContactService, IGenericRepository<GCTL.Data.Models.EmployeeFamilyInfo> employeeFamilyInfoRepository, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2, IElementPermissionService elementPermissionService) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeContactService = employeeContactService;
            _employeeFamilyInfoRepository = employeeFamilyInfoRepository;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository2 = userManagerRepository2;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
            _elementPermissionService = elementPermissionService;
        }

        public async Task<IActionResult> Index(int id)
        {

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
            SetSmartPageCode(117000);

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


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "EmergencyContact");
                ViewBag.Navigation = navigationModel;

                var pageId = 2;
                var elementKey = "EmployeeDropDown";

                bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, pageId, elementKey);


               // bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeTable");

                if (!hasEmployeePermission)
                {
                    var empid = loggedUser.EmployeeId;
                    ViewBag.empId = empid;
                }
                else
                {
                    ViewBag.empId = id;
                }



            }


          

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(EmployeeContactViewModel model)
        {
            var res = await _employeeContactService.SaveAsync(model);
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> SubmitFromEdit(EmployeeContactViewModel model)
        {
            var res = await _employeeContactService.SaveAsync(model);
            var employee = await _employeeContactService.GetEmployeeContactByIdAsync(model.EmployeePersonalId);
            res.Data = employee;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _employeeContactService.DeleteAsync(id);
            if (res.Success)
            {
                var employee = await _employeeContactService.GetEmployeeContactByIdAsync(Convert.ToInt32(res.Data));
                res.Data = employee;
            }
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFromEdit(int id)
        {
            var res = await _employeeContactService.DeleteAsync(id);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeData(int id)
        {

            var loggedUser = await _userManagerRepository2.GetUserAsync(User);

            if (loggedUser != null)
            {
                var userId = loggedUser.Id;

                var pageId = 2;
                var elementKey = "EmployeeDropDown";

                bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, pageId, elementKey);


               // bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeTable");

                if (!hasEmployeePermission)
                {
                    var empid = loggedUser.EmployeeId;

                    if (empid == null || empid == 0)
                    {
                        return Ok();

                    }
                    else
                    {

                        
                        var employee = await _employeeContactService.GetEmployeeContactByIdAsync((int)empid);
                        return Ok(employee);
                    }


                }
                else
                {

                    var employee = await _employeeContactService.GetEmployeeContactByIdAsync(id);
                    return Ok(employee);
                }


            }

            return Ok();

           
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeContactData(int id)
        {
            try
            {

                var loggedUser = await _userManagerRepository2.GetUserAsync(User);

                if (loggedUser != null)
                {
                    var userId = loggedUser.Id;

                    var pageId = 2;
                    var elementKey = "EmployeeDropDown";

                    bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, pageId, elementKey);


                   // bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeTable");

                    if (!hasEmployeePermission)
                    {
                        var empid = loggedUser.EmployeeId;

                        if (empid == null || empid == 0)
                        {
                            return Ok();

                        }
                        else
                        {


                            var data = await _employeeContactService.GetEmployeeContactData((int)empid);
                            return Ok(data);
                        }


                    }
                    else
                    {

                        var data = await _employeeContactService.GetEmployeeContactData(id);
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
        public async Task<IActionResult> Update([FromBody] EmployeeContactViewModel model)
        {
            if (model == null || model.EmployeeEmeContactID <= 0)
            {
                return Ok(new { success = false, message = "Invalid data" });
            }

            var res = await _employeeContactService.UpdateAsync(model);
            return Ok(res);
        }

        [HttpGet]
        public IActionResult GetContactSuggestions(string term, int id)
        {
            var contacts = _employeeFamilyInfoRepository.AllActive().Where(e=>e.EmployeeID == id).ToList().Select(e => new ContactSuggestionViewModel
            {
                Id = e.EmployeeFamilyInfoID,
                Name = e.FullName,
                Relationship = e.RelationToEmployee,
                ContactNumber = e.ContactNumber,
                ContactEmail = e.Email
            }).ToList();

            var filteredContacts = string.IsNullOrEmpty(term)
                ? contacts
                : contacts.Where(c => c.Name != null && c.Name.ToLower().Contains(term.ToLower())).ToList();

            return Json(filteredContacts.Select(c => new
            {
                id = c.Id,
                label = c.Name,
                value = c.Name,
                relationship = c.Relationship ?? string.Empty,
                contactNumber = c.ContactNumber ?? string.Empty,
                contactEmail = c.ContactEmail ?? string.Empty
            }));
        }

    }
}
