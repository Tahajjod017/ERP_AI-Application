using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeePersonalController : BaseController
    {
        private readonly IEmployeePersonalService _employeePersonalService;
        private readonly IGenericRepository<MaritalStatus> _maritalRepository;
        private readonly IGenericRepository<Religions> _religionRepository;
        private readonly IGenericRepository<Genders> _genderRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<BloodGroup> _bloodGroupRepository;
        public EmployeePersonalController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeePersonalService employeePersonalService, IGenericRepository<MaritalStatus> maritalRepository, IGenericRepository<Religions> religionRepository, IGenericRepository<Genders> genderRepository, IGenericRepository<Country> countryRepository, IGenericRepository<BloodGroup> bloodGroupRepository) : base(translateService, userProfileService)
        {
            _employeePersonalService = employeePersonalService;
            _maritalRepository = maritalRepository;
            _religionRepository = religionRepository;
            _genderRepository = genderRepository;
            _countryRepository = countryRepository;
            _bloodGroupRepository = bloodGroupRepository;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(111000);

            PopulateViewBag();

           
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


    }
}
