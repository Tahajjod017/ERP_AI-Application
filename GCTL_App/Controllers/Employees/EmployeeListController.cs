
using System.Linq;
using System.Web.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeListVM;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeAdditional;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Employees.EmployeeList;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Employees.EmployeeSalary;
using GCTL.Service.Employees.EmployeeTraining;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeListController : BaseController
    {

        #region CTOR

        private readonly IEmployeeListService _employeeListService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IEmployeeAdditionalService _employeeAdditionalService;
        private readonly IEmployeeAllowanceService _employeeAllowanceService;
        private readonly IEmployeeBenifitService employeeBenifitService;
        private readonly IEmployeeContactService _employeeContactService;
        private readonly IEmployeeEducationalService _employeeEducationalService;
        private readonly IEmployeeFamilyService _employeeFamilyService; 
        private readonly IEmployeeOfficialService _employeeOfficialService;
        private readonly IEmployeePersonalService _employeePersonalService;
        private readonly IEmployeeSalaryService _employeeSalaryService;
        private readonly IEmployeeTrainingService _employeeTrainingService;
        private readonly IGenericRepository<EmployeeType> _employeeTypeRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<EmploymentNature> _employmentNatureRepository;
        private readonly IGenericRepository<Statuses> _employeeStatusRepository;

        private readonly IGenericRepository<Organization> _companyRepository;
        private readonly IGenericRepository<LicenceTypes> _licenceTypesRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeOfficeInfo> _employeeOfficeRepository;
        private readonly IGenericRepository<YearlyEndBonusTypes> _yearlyEndBonusTypesRepository;
        private readonly IGenericRepository<ServiceYears> _serviceYearsRepository;
        private readonly IGenericRepository<EducationLevels> _educationLevelsRepository;
        private readonly IGenericRepository<Degree> _degreeRepository;
        private readonly IGenericRepository<EducationBoard> _educationBoardRepository;
        private readonly IGenericRepository<ResultTypes> _resultTypeRepository;
        private readonly IGenericRepository<PassingYears> _passingYearRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<OrganizationBranches> _branchRepository;
        private readonly IGenericRepository<Grade> _gradeRepository;
        private readonly IGenericRepository<Currencies> _currencyRepository;
        private readonly IGenericRepository<PaymentPeriodTypes> _paymentPeriodTypeRepository;
        private readonly IGenericRepository<PaymentModes> _paymentModeRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<MaritalStatus> _maritalRepository;
        private readonly IGenericRepository<Religions> _religionRepository;
        private readonly IGenericRepository<TrainingYears> _trainingYearsRepository;
        private readonly IGenericRepository<Genders> _genderRepository;
        private readonly IGenericRepository<BloodGroup> _bloodGroupRepository;
        private readonly IGenericRepository<ProvisionPeriodTtimeTypes> _provisionPeriodTtimeTypesRepository;



        public EmployeeListController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeListService employeeListService, IHttpContextAccessor httpContextAccessor, IEmployeeAdditionalService employeeAdditionalService, IEmployeeAllowanceService employeeAllowanceService, IEmployeeBenifitService employeeBenifitService, IEmployeeContactService employeeContactService, IEmployeeEducationalService employeeEducationalService, IEmployeeFamilyService employeeFamilyService, IEmployeeOfficialService employeeOfficialService, IEmployeePersonalService employeePersonalService, IEmployeeSalaryService employeeSalaryService, IEmployeeTrainingService employeeTrainingService, IGenericRepository<LicenceTypes> licenceTypesRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypesRepository, IGenericRepository<ServiceYears> serviceYearsRepository, IGenericRepository<EducationLevels> educationLevelsRepository, IGenericRepository<Degree> degreeRepository, IGenericRepository<EducationBoard> educationBoardRepository, IGenericRepository<ResultTypes> resultTypeRepository, IGenericRepository<PassingYears> passingYearRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<OrganizationBranches> branchRepository, IGenericRepository<EmployeeType> employeeTypeRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmploymentNature> employmentNatureRepository, IGenericRepository<Statuses> employeeStatusRepository, IGenericRepository<Grade> gradeRepository, IGenericRepository<Currencies> currencyRepository, IGenericRepository<PaymentPeriodTypes> paymentPeriodTypeRepository, IGenericRepository<PaymentModes> paymentModeRepository, IGenericRepository<Country> countryRepository, IGenericRepository<MaritalStatus> maritalRepository, IGenericRepository<Religions> religionRepository, IGenericRepository<TrainingYears> trainingYearsRepository, IGenericRepository<Genders> genderRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeRepository, IGenericRepository<BloodGroup> bloodGroupRepository, IGenericRepository<ProvisionPeriodTtimeTypes> provisionPeriodTtimeTypesRepository, IGenericRepository<Organization> companyRepository) : base(translateService, userProfileService)
        {
            _employeeListService = employeeListService;
            _httpContextAccessor = httpContextAccessor;
            _employeeAdditionalService = employeeAdditionalService;
            _employeeAllowanceService = employeeAllowanceService;
            this.employeeBenifitService = employeeBenifitService;
            _employeeContactService = employeeContactService;
            _employeeEducationalService = employeeEducationalService;
            _employeeFamilyService = employeeFamilyService;
            _employeeOfficialService = employeeOfficialService;
            _employeePersonalService = employeePersonalService;
            _employeeSalaryService = employeeSalaryService;
            _employeeTrainingService = employeeTrainingService;
            _licenceTypesRepository = licenceTypesRepository;
            _employeeRepository = employeeRepository;
            _yearlyEndBonusTypesRepository = yearlyEndBonusTypesRepository;
            _serviceYearsRepository = serviceYearsRepository;
            _educationLevelsRepository = educationLevelsRepository;
            _degreeRepository = degreeRepository;
            _educationBoardRepository = educationBoardRepository;
            _resultTypeRepository = resultTypeRepository;
            _passingYearRepository = passingYearRepository;
            _organizationRepository = organizationRepository;
            _branchRepository = branchRepository;
            _employeeTypeRepository = employeeTypeRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _employmentNatureRepository = employmentNatureRepository;
            _employeeStatusRepository = employeeStatusRepository;
            _gradeRepository = gradeRepository;
            _currencyRepository = currencyRepository;
            _paymentPeriodTypeRepository = paymentPeriodTypeRepository;
            _paymentModeRepository = paymentModeRepository;
            _countryRepository = countryRepository;
            _maritalRepository = maritalRepository;
            _religionRepository = religionRepository;
            _trainingYearsRepository = trainingYearsRepository;
            _genderRepository = genderRepository;
            _employeeOfficeRepository = employeeOfficeRepository;
            _bloodGroupRepository = bloodGroupRepository;
            _provisionPeriodTtimeTypesRepository = provisionPeriodTtimeTypesRepository;
            _companyRepository = companyRepository;
        }

        #endregion




        public IActionResult Index()
        {
            PopulateDDViewBag();
            SetSmartPageCode(118000);
            return View();
        }

        private void PopulateDDViewBag()
        {
            #region ViewBag

            ViewBag.TotalEmployee = _employeeOfficeRepository.AllActive().Count();
            ViewBag.ActiveEmployee = _employeeOfficeRepository.AllActive().Where(e => e.EmploymentStatusId == 1007).Count();
            ViewBag.InactiveEmployee = _employeeOfficeRepository.AllActive().Where(e => e.EmploymentStatusId == 1008).Count();

            DateOnly threeMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3));
            var newJoinings = _employeeOfficeRepository.AllActive().Where(emp => emp.JoiningDate.HasValue && emp.JoiningDate.Value >= threeMonthsAgo).ToList();
            ViewBag.NewJoinings = newJoinings.Count();

            ViewBag.CompanyDD = new SelectList(_companyRepository.AllActive().Select(d => new { d.OrganizationID, d.OrganizationName }), "OrganizationID", "OrganizationName");
            ViewBag.DepartmentDD = new SelectList(_departmentRepository.AllActive().Select(d => new { d.DepartmentID, d.DepartmentName }), "DepartmentID", "DepartmentName");
            ViewBag.LicenseTypeDD = _licenceTypesRepository.GetSelectListById(e => e.LicenceTypeID, e => e.LicenceTypeName);
            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
            ViewBag.CountryDD = new SelectList(_countryRepository.AllActive(), "CountryID", "CountryName");


            ViewBag.HouseRentAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "35", Text = "35 %" },
                new SelectListItem { Value = "40", Text = "40 %" },
                new SelectListItem { Value = "45", Text = "45 %" },
                new SelectListItem { Value = "50", Text = "50 %" },
                new SelectListItem { Value = "60", Text = "60 %" },
                new SelectListItem { Value = "70", Text = "70 %" },
                new SelectListItem { Value = "100", Text = "100 %" }
            }, "Value", "Text");

            ViewBag.MedicalAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "8", Text = "8 %" },
                new SelectListItem { Value = "10", Text = "10 %" },
                new SelectListItem { Value = "15", Text = "15 %" }
            }, "Value", "Text");

            ViewBag.ConveyanceAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "8", Text = "8 %" },
                new SelectListItem { Value = "10", Text = "10 %" },
                new SelectListItem { Value = "15", Text = "15 %" }
            }, "Value", "Text");





            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");

            ViewBag.YearlyEndBonusTypeDD = new SelectList(_yearlyEndBonusTypesRepository.AllActive().Select(e => new { e.YearlyEndBonusTypeID, e.YearlyEndBonusTypeName }), "YearlyEndBonusTypeID", "YearlyEndBonusTypeName");

            ViewBag.ServiceYearDD = new SelectList(_serviceYearsRepository.AllActive().Select(e => new { e.ServiceYearID, e.ServiceYearName }), "ServiceYearID", "ServiceYearName");

            ViewBag.FastivalBonusPercentageDD = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "35", Text = "35 %" },
                    new SelectListItem { Value = "40", Text = "40 %" },
                    new SelectListItem { Value = "45", Text = "45 %" },
                    new SelectListItem { Value = "50", Text = "50 %" },
                    new SelectListItem { Value = "60", Text = "60 %" },
                    new SelectListItem { Value = "70", Text = "70 %" },
                    new SelectListItem { Value = "100", Text = "100 %" }
                }, "Value", "Text");

            ViewBag.BonusDependsOnDD = new SelectList(new[]
                {
                    new { Value = "Gross Salary", Text = "Gross Salary" },
                    new { Value = "Basic Salary", Text = "Basic Salary" }
                }, "Value", "Text");


            ViewBag.PFEmployeeContributionDD = new SelectList(new[]
                {
                    new { Value = "5", Text = "5 %" },
                    new { Value = "6", Text = "6 %" },
                    new { Value = "7", Text = "7 %" },
                    new { Value = "8", Text = "8 %" },
                    new { Value = "9", Text = "9 %" },
                    new { Value = "10", Text = "10 %" }
                }, "Value", "Text");


            ViewBag.PFOrgContributionDD = new SelectList(new[]
                {
                    new { Value = "5", Text = "5 %" },
                    new { Value = "6", Text = "6 %" },
                    new { Value = "7", Text = "7 %" },
                    new { Value = "8", Text = "8 %" },
                    new { Value = "9", Text = "9 %" },
                    new { Value = "10", Text = "10 %" }
                }, "Value", "Text");


            ViewBag.PFDependsOnDD = new SelectList(new[]
                {
                    new { Value = "Gross Salary", Text = "Gross Salary" },
                    new { Value = "Basic Salary", Text = "Basic Salary" }
                }, "Value", "Text");





            ViewBag.EducationLevel = _educationLevelsRepository.GetActiveSelectListById(e => e.EducationLevelID, e => e.EducationLevelName);
            ViewBag.Degree = _degreeRepository.GetActiveSelectListById(e => e.DegreeID, e => e.DegreeName);
            ViewBag.EducationBoard = _educationBoardRepository.GetActiveSelectListById(e => e.EducationBoardID, e => e.EducationBoardName);
            ViewBag.ResultType = _resultTypeRepository.GetActiveSelectListById(e => e.ResultTypeID, e => e.ResultTypeName);
            ViewBag.PassingYear = _passingYearRepository.GetActiveSelectListById(e => e.PassingYearID, e => e.PassingYearName);



            ViewBag.EmployeeDD = new SelectList(
                _employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.OrganizationDD = new SelectList(
                _organizationRepository.AllActive().Select(o => new { o.OrganizationID, o.OrganizationName }),
                "OrganizationID",
                "OrganizationName"
            );

            ViewBag.BranchDD = new SelectList(
                _branchRepository.AllActive().Select(b => new { b.OrganizationBranchID, b.OrganizationBranchName }),
                "OrganizationBranchID",
                "OrganizationBranchName"
            );

            ViewBag.EmployeeTypeDD = new SelectList(
                _employeeTypeRepository.AllActive().Select(et => new { et.EmployeeTypeID, et.EmployeeTypeName }),
                "EmployeeTypeID",
                "EmployeeTypeName"
            );

            ViewBag.DepartmentDD = new SelectList(
                _departmentRepository.AllActive().Select(d => new { d.DepartmentID, d.DepartmentName }),
                "DepartmentID",
                "DepartmentName"
            );

            ViewBag.DesignationDD = new SelectList(
                _designationRepository.AllActive().Select(d => new { d.DesignationID, d.DesignationName }),
                "DesignationID",
                "DesignationName"
            );

            ViewBag.EmploymentNatureDD = new SelectList(
                _employmentNatureRepository.AllActive().Select(en => new { en.EmploymentNatureID, en.EmploymentNatureName }),
                "EmploymentNatureID",
                "EmploymentNatureName"
            );

            ViewBag.SeniorSupervisorDD = new SelectList(
                _employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.ImmediateSupervisorDD = new SelectList(
                _employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.HeadOfDepartmentDD = new SelectList(
                _employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.EmployeeStatusDD = new SelectList(
                _employeeStatusRepository.AllActive().Select(es => new { es.StatusID, es.StatusName }),
                "StatusID",
                "StatusName"
            );

            ViewBag.BloodGroupDD = new SelectList(_bloodGroupRepository.AllActive(), "BloodGroupID", "BloodGroupName");

            ViewBag.TimeUnitDD = new SelectList(_provisionPeriodTtimeTypesRepository.AllActive().Select(tu => new { tu.ProvisionPeriodTtimeTypeID, tu.ProvisionPeriodTtimeTypeName }), "ProvisionPeriodTtimeTypeID", "ProvisionPeriodTtimeTypeName");


            //ViewBag.TimeUnitDD = new SelectList(new List<object>{
            //    new { TimeUnitID = 1, TimeUnitName = "Days" },
            //    new { TimeUnitID = 2, TimeUnitName = "Months" },
            //    new { TimeUnitID = 3, TimeUnitName = "Years" }
            //}, "TimeUnitID", "TimeUnitName");


            ViewBag.MaritalStatusDD = new SelectList(_maritalRepository.AllActive(), "MaritalStatusID", "MaritalStatusName");
            ViewBag.ReligionDD = new SelectList(_religionRepository.AllActive(), "ReligionID", "ReligionName");
            ViewBag.GenderDD = new SelectList(_genderRepository.AllActive(), "GenderID", "GenderName");

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
            ViewBag.GradeDD = new SelectList(_gradeRepository.AllActive().Select(o => new { o.GradeID, o.GradeName }), "GradeID", "GradeName");
            ViewBag.CurrencyDD = new SelectList(_currencyRepository.AllActive().Select(o => new { o.CurrencyID, o.CurrencyName }), "CurrencyID", "CurrencyName");
            ViewBag.PaymenPeriodTypeDD = new SelectList(_paymentPeriodTypeRepository.AllActive().Select(o => new { o.PaymentPeriodTypeID, o.PaymentPeriodTypeName }), "PaymentPeriodTypeID", "PaymentPeriodTypeName");
            ViewBag.PaymenModeDD = new SelectList(_paymentModeRepository.AllActive().Select(o => new { o.PaymentModeID, o.PaymentModeName }), "PaymentModeID", "PaymentModeName");

            ViewBag.Country = _countryRepository.GetActiveSelectListById(c => c.CountryID, c => c.CountryName);
            ViewBag.TrainingYear = _trainingYearsRepository.GetActiveSelectListById(t => t.TrainingYearID, t => t.TrainingYearName);

            #endregion

        }

        //public IActionResult GetLocalHost()
        //{
        //    var request = _httpContextAccessor.HttpContext.Request;
        //    string url = $"{request.Scheme}://{request.Host}";

        //    return Ok(new { LocalHostUrl = url });
        //}


        #region Populate Edit Form

        [HttpGet]
        public async Task<IActionResult> GetEmployeePersonal(int id) => Ok(await _employeePersonalService.GetEmployeePersonalById(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeOfficial(int id) => Ok(await _employeeOfficialService.GetEmployeeOfficalDetails(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeAdditional(int id) => Ok(await _employeeAdditionalService.GetEmployeeAdditionalByIdAsync(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeContact(int id) => Ok(await _employeeContactService.GetEmployeeContactByIdAsync(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeEducational(int id) => Ok(await _employeeEducationalService.GetEmployeeAdditionalByIdAsync(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeFamily(int id) => Ok(await _employeeFamilyService.GetEmployeeFamilyByIdAsync(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeSalary(int id) => Ok(await _employeeSalaryService.GetEmployeeSalaryByEmployeeIdAsync(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeTraining(int id) => Ok(await _employeeTrainingService.GetEmployeeTrainingByIdAsync(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeAllowance(int id) => Ok(await _employeeAllowanceService.GetEmployeeAllowance(id));

        [HttpGet]
        public async Task<IActionResult> GetEmployeeBenefit(int id) => Ok(await employeeBenifitService.GetEmployeeBenefitsAsync(id.ToString()));

        #endregion

        #region GetEmployee For Table and Board


        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] int page = 1, [FromQuery] int limit = 3, [FromQuery] string department = "",
        [FromQuery] string status = "",[FromQuery] string sort = "",[FromQuery] string search = "",[FromQuery] string sortColumn = "joiningDate",
        [FromQuery] string sortDirection = "desc", [FromQuery] string company = "") // Added company parameter
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string url = $"{request.Scheme}://{request.Host}";
                string url1 = GetEmployeePictureURL(true);

                // Build query
                IQueryable<EmployeeListGetViewModel> query = await _employeeListService.GetEmployees();


                if (!string.IsNullOrEmpty(company))
                {
                    var companyIds = company.Split(',').Select(id => long.Parse(id.Trim())).ToList();
                    query = query.Where(e => e.CompanyId.HasValue && companyIds.Contains(e.CompanyId.Value));
                }

             

                //if (!string.IsNullOrEmpty(company))
                //{
                //    var companyIds = company.Split(',').Select(id => Convert.ToInt64(id.Trim())).ToList();

                //    query = query.Where(e => companyIds.Contains(Convert.ToInt64(e.CompanyId)));

                //    //query = query.Where(e => e.CompanyId.ToString() == company); // Adjust based on your EmployeeListGetViewModel
                //}

                // Filter by department
                if (!string.IsNullOrEmpty(department))
                {
                    var departmentIds = department.Split(',').Select(id => Convert.ToInt64(id.Trim())).ToList();

                    query = query.Where(e => departmentIds.Contains(e.DepartmentId));



                    //var dept = _departmentRepository.AllActive().Where(e => e.DepartmentID == Convert.ToInt64(department)).Select(e => e.DepartmentName).FirstOrDefault();
                    //query = query.Where(e => e.Department == dept);
                }

                

                // Filter by status
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(e => e.Status == status);
                }

                

                // Filter by company
                

                // Search by name or email
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(e => e.Name.ToLower().Contains(search.ToLower()) || e.Email.ToLower().Contains(search.ToLower()));
                }

                // Date filter for sort
                string lowerSort = sort.ToLowerInvariant();
                DateOnly? dateFilter = lowerSort switch
                {
                    "sort by last 7 days" => DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
                    "sort by last 15 days" => DateOnly.FromDateTime(DateTime.Today.AddDays(-15)),
                    "sort by last 1 month" => DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
                    "sort by last 3 month" => DateOnly.FromDateTime(DateTime.Today.AddMonths(-3)),
                    "sort by last 6 month" => DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)),
                    _ => null
                };

                if (dateFilter.HasValue)
                {
                    query = query.Where(e => e.JoiningDate >= dateFilter.Value);
                }

                // Apply sorting
                query = sortColumn.ToLower() switch
                {
                    "empid" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.Id) : query.OrderByDescending(e => e.Id),
                    "empname" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name),
                    "empemail" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.Email) : query.OrderByDescending(e => e.Email),
                    "empphone" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.Phone) : query.OrderByDescending(e => e.Phone),
                    "empdesignation" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.Department) : query.OrderByDescending(e => e.Department),
                    "empjointindate" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.JoiningDate) : query.OrderByDescending(e => e.JoiningDate),
                    "empstatus" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.Status) : query.OrderByDescending(e => e.Status),
                    _ => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.JoiningDate) : query.OrderByDescending(e => e.JoiningDate)
                };

                // Get total count for pagination
                int total = await query.CountAsync();

                // Apply pagination
                int skip = (page - 1) * limit;
                query = query.Skip(skip).Take(limit);

                // Execute query
                var employees = await query
                    .Select(e => new
                    {
                        id = e.Id,
                        name = string.IsNullOrEmpty(e.Name) ? "-" : e.Name,
                        email = string.IsNullOrEmpty(e.Email) ? "-" : e.Email,
                        phone = string.IsNullOrEmpty(e.Phone) ? "-" : e.Phone,
                        department = string.IsNullOrEmpty(e.Department) ? "-" : e.Department,
                        joiningDate = e.JoiningDate.HasValue ? e.JoiningDate : null,
                        status = string.IsNullOrEmpty(e.Status) ? "-" : e.Status,
                        avatar = string.IsNullOrEmpty(e.Avatar) ? null : url1 + e.Avatar,
                        url = url + "/Uploads/employee/"
                    })
                    .ToListAsync();

                // Return response
                return Ok(new
                {
                    employees,
                    total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult GetDepartmentsByOrgId(string organizationId)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationId))
                {
                    return BadRequest("Organization ID is required");
                }

                var companyIds = organizationId.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => long.Parse(id.Trim()))
                    .ToList();

                if (!companyIds.Any())
                {
                    return BadRequest("Valid organization IDs are required");
                }

                var departments = _departmentRepository.AllActive()
                    .Where(e => e.OrganizationID.HasValue && companyIds.Contains(e.OrganizationID.Value))
                    .Select(d => new {
                        departmentID = d.DepartmentID,
                        departmentName = d.DepartmentName
                    })
                    .OrderBy(d => d.departmentName)
                    .ToList();

                return Ok(departments);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetDepartmentsByOrgId: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching departments");
            }
        }



        [HttpGet]
        public IActionResult GetAllDepartments()
        {
            try
            {
                var departments = _departmentRepository.AllActive()
                    .Select(d => new {
                        departmentID = d.DepartmentID,
                        departmentName = d.DepartmentName
                    })
                    .OrderBy(d => d.departmentName)
                    .ToList();

                return Ok(departments);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetAllDepartments: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching all departments");
            }
        }



        //[HttpGet]
        //public IActionResult GetDepartmentsByOrgId(string organizationId)
        //{
        //    var companyIds = organizationId.Split(',').Select(id => long.Parse(id.Trim())).ToList();



        //    var departments = _departmentRepository.AllActive().Where(e => e.OrganizationID.HasValue && companyIds.Contains(e.OrganizationID.Value))
        //        .Select(d => new {
        //            departmentID = d.DepartmentID,
        //            departmentName = d.DepartmentName
        //        }).ToList();

        //    return Ok(departments);
        //}

        #endregion
    }
}
