

using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Language;


using GCTL.Service.UserProfile;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol.Core.Types;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeEducationController : BaseController
    {

        private readonly IEmployeeEducationalService _employeeEducationalService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EducationLevels> _educationLevelsRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Degree> _degreeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EducationBoard> _educationBoardRepository;
        private readonly IGenericRepository<GCTL.Data.Models.ResultTypes> _resultTypeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.PassingYears> _passingYearRepository;


        public EmployeeEducationController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeEducationalService employeeEducationalService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<GCTL.Data.Models.EducationLevels> educationLevelsRepository, IGenericRepository<GCTL.Data.Models.Degree> degreeRepository, IGenericRepository<GCTL.Data.Models.EducationBoard> educationBoardRepository, IGenericRepository<GCTL.Data.Models.ResultTypes> resultTypeRepository, IGenericRepository<GCTL.Data.Models.PassingYears> passingYearRepository) : base(translateService, userProfileService)
        {
            _employeeEducationalService = employeeEducationalService;
            _employeeRepository = employeeRepository;
            _educationLevelsRepository = educationLevelsRepository;
            _degreeRepository = degreeRepository;
            _educationBoardRepository = educationBoardRepository;
            _resultTypeRepository = resultTypeRepository;
            _passingYearRepository = passingYearRepository;
        }

        public IActionResult Index(int id)
        {
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



            var res = await _employeeEducationalService.SubmitAsync(model);
            return Ok(res);



        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {



            var res = await _employeeEducationalService.DeleteAsync(id);
            return Ok(res);



        }



        [HttpGet]
        public async Task<IActionResult> GetEmployeeData(int id)
        {

            var employee = await _employeeEducationalService.GetEmployeeAdditionalByIdAsync(id);

            return Ok(employee);
        }

     
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

    }
}
