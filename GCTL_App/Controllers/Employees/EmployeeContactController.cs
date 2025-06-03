using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeContact;
using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeContactController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeFamilyInfo> _employeeFamilyInfoRepository;

        private readonly IEmployeeContactService _employeeContactService;

        public EmployeeContactController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IEmployeeContactService employeeContactService, IGenericRepository<GCTL.Data.Models.EmployeeFamilyInfo> employeeFamilyInfoRepository) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeContactService = employeeContactService;
            _employeeFamilyInfoRepository = employeeFamilyInfoRepository;
        }

        public IActionResult Index(int id)
        {
            ViewBag.EmployeeDD = new SelectList(
                _employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );
            SetSmartPageCode(117000);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(EmployeeContactViewModel model)
        {
            var res = await _employeeContactService.SaveAsync(model);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _employeeContactService.DeleteAsync(id);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeData(int id)
        {
            var employee = await _employeeContactService.GetEmployeeContactByIdAsync(id);
            return Ok(employee);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeContactData(int id)
        {
            try
            {
                var data = await _employeeContactService.GetEmployeeContactData(id);
                return Ok(data);
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
            var contacts = _employeeFamilyInfoRepository.All().Where(e=>e.EmployeeID == id).ToList().Select(e => new ContactSuggestionViewModel
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
