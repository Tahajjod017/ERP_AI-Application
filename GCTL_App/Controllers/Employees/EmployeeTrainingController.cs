
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Core.ViewModels.Employee.EmployeeTraining;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeTraining;
using GCTL.Service.Language;


using GCTL.Service.UserProfile;

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

        public EmployeeTrainingController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeTrainingService employeeTrainingService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Country> countryRepository, IGenericRepository<TrainingYears> trainingYearsRepository) : base(translateService, userProfileService)
        {
            _employeeTrainingService = employeeTrainingService;
            _employeeRepository = employeeRepository;
            _countryRepository = countryRepository;
            _trainingYearsRepository = trainingYearsRepository;
        }

        public IActionResult Index()
        {
            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
            ViewBag.Country = _countryRepository.GetActiveSelectListById(c => c.CountryID, c => c.CountryName);
            ViewBag.TrainingYear = _trainingYearsRepository.GetActiveSelectListById(t => t.TrainingYearID, t => t.TrainingYearName);
            SetSmartPageCode(115000);
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmployeeTrainingPostViewModel model)
        {
            var res = await _employeeTrainingService.SaveAsync(model);
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _employeeTrainingService.DeleteAsync(id);
            return Ok(res);
        }



        [HttpGet]
        public async Task<IActionResult> GetEmployeeData(int id)
        {

            var employee = await _employeeTrainingService.GetEmployeeTrainingByIdAsync(id);

            return Ok(employee);
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
