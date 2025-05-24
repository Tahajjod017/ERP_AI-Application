using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeePersonalController : BaseController
    {
        private readonly IEmployeePersonalService _employeePersonalService;
        private readonly IGenericRepository<MaritalStatus> _maritalRepository;
        public EmployeePersonalController(ITranslateService translateService, IEmployeePersonalService employeePersonalService, IGenericRepository<MaritalStatus> maritalRepository) : base(translateService)
        {
            _employeePersonalService = employeePersonalService;
            _maritalRepository = maritalRepository;
        }

        public IActionResult Index()
        {
            // ViewBag.MaritalStatusDD = new SelectList(_maritalRepository.All(), "MaritalStatusId", "MaritalStatusName");
            ViewBag.MaritalStatusDD = _maritalRepository.All().ToList();

            SetSmartPageCode(111000);
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePersonalInfo(EmployeePersonalPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save and get the new employee ID
                CommonReturnViewModel result = await _employeePersonalService.SaveEmployeePersonalInfo(model);

                if (result.Success) // Assuming `Id` holds the new employee's ID
                {
                    return RedirectToAction("Index", "EmployeeOfficial", new { id = result.Data });
                }

                // Handle failure case if needed
                ModelState.AddModelError(string.Empty, result.Message ?? "Failed to save employee info.");
            }

            // If validation fails, return the same view with the model to display errors
            return View(model);
        }

    }
}
