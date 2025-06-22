using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeDetails;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeAdditional;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Employees.EmployeeSalary;
using GCTL.Service.Employees.EmployeeTraining;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GCTL.Service.Employees.EmployeeDetails
{
    public class EmployeeDetailsService : IEmployeeDetailsService
    {
        #region CTOR


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

        private readonly IGenericRepository<LicenceTypes> _licenceTypesRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeAdditionalInfo> _employeeAdditionalRepository;
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

        public EmployeeDetailsService(IEmployeeAdditionalService employeeAdditionalService, IEmployeeAllowanceService employeeAllowanceService, IEmployeeBenifitService employeeBenifitService, IEmployeeContactService employeeContactService, IEmployeeEducationalService employeeEducationalService, IEmployeeFamilyService employeeFamilyService, IEmployeeOfficialService employeeOfficialService, IEmployeePersonalService employeePersonalService, IEmployeeSalaryService employeeSalaryService, IEmployeeTrainingService employeeTrainingService, IGenericRepository<EmployeeType> employeeTypeRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmploymentNature> employmentNatureRepository, IGenericRepository<Statuses> employeeStatusRepository, IGenericRepository<LicenceTypes> licenceTypesRepository, IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeRepository, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypesRepository, IGenericRepository<ServiceYears> serviceYearsRepository, IGenericRepository<EducationLevels> educationLevelsRepository, IGenericRepository<Degree> degreeRepository, IGenericRepository<EducationBoard> educationBoardRepository, IGenericRepository<ResultTypes> resultTypeRepository, IGenericRepository<PassingYears> passingYearRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<OrganizationBranches> branchRepository, IGenericRepository<Grade> gradeRepository, IGenericRepository<Currencies> currencyRepository, IGenericRepository<PaymentPeriodTypes> paymentPeriodTypeRepository, IGenericRepository<PaymentModes> paymentModeRepository, IGenericRepository<Country> countryRepository, IGenericRepository<MaritalStatus> maritalRepository, IGenericRepository<Religions> religionRepository, IGenericRepository<TrainingYears> trainingYearsRepository, IGenericRepository<Genders> genderRepository, IGenericRepository<BloodGroup> bloodGroupRepository, IGenericRepository<ProvisionPeriodTtimeTypes> provisionPeriodTtimeTypesRepository, IGenericRepository<EmployeeAdditionalInfo> employeeAdditionalRepository)
        {
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
            _employeeTypeRepository = employeeTypeRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _employmentNatureRepository = employmentNatureRepository;
            _employeeStatusRepository = employeeStatusRepository;
            _licenceTypesRepository = licenceTypesRepository;
            _employeeRepository = employeeRepository;
            _employeeOfficeRepository = employeeOfficeRepository;
            _yearlyEndBonusTypesRepository = yearlyEndBonusTypesRepository;
            _serviceYearsRepository = serviceYearsRepository;
            _educationLevelsRepository = educationLevelsRepository;
            _degreeRepository = degreeRepository;
            _educationBoardRepository = educationBoardRepository;
            _resultTypeRepository = resultTypeRepository;
            _passingYearRepository = passingYearRepository;
            _organizationRepository = organizationRepository;
            _branchRepository = branchRepository;
            _gradeRepository = gradeRepository;
            _currencyRepository = currencyRepository;
            _paymentPeriodTypeRepository = paymentPeriodTypeRepository;
            _paymentModeRepository = paymentModeRepository;
            _countryRepository = countryRepository;
            _maritalRepository = maritalRepository;
            _religionRepository = religionRepository;
            _trainingYearsRepository = trainingYearsRepository;
            _genderRepository = genderRepository;
            _bloodGroupRepository = bloodGroupRepository;
            _provisionPeriodTtimeTypesRepository = provisionPeriodTtimeTypesRepository;
            _employeeAdditionalRepository = employeeAdditionalRepository;
        }

        #endregion

        public async Task<CommonReturnViewModel> GetBasicDetail(int empID, string imgURL)
        {
            try
            {
                var query = await (from e in _employeeRepository.AllActive()
                                   where e.EmployeeID == empID
                                   join o in _employeeOfficeRepository.AllActive() on e.EmployeeID equals o.EmployeeID into officeGroup
                                   from office in officeGroup.DefaultIfEmpty()
                                   join a in _employeeAdditionalRepository.AllActive() on e.EmployeeID equals a.EmployeeID into additionalGroup
                                   from additional in additionalGroup.DefaultIfEmpty()
                                   join s in _employeeRepository.AllActive() on (office != null ? office.ImmediateSupervisorId : (int?)null) equals s.EmployeeID into supervisorGroup
                                   from supervisor in supervisorGroup.DefaultIfEmpty()
                                   select new EmployeeBasicDetailsViewModel
                                   {
                                       EmployeeId = e.EmployeeID,
                                       Name = e.FirstName + " " + e.LastName,
                                       Email = e.Email,
                                       Phone = office != null ? office.OfficePhone : "-",
                                       Gender = e.Gender != null ? e.Gender.GenderName : "-",
                                       DateOfBirth = e.DateOfBirth,
                                       Address = e.HouseNo + ", " + e.RoadNo + ", " + e.City + ", " + e.State + ", " +
                                                (e.Country != null ? e.Country.CountryName : ""),


                                       Image = imgURL + e.EmployeeImageFileName,

                                       Bio = e.AboutEmployee,

                                       Role = office != null && office.Designation != null ? office.Designation.DesignationName : "-",
                                       Department = office != null && office.Department != null ? office.Department.DepartmentName : "-",
                                       JoinDate = office != null ? office.JoiningDate : null,

                                       // Calculate experience - will be handled in post-processing
                                       Experience = office != null && office.JoiningDate != null ? "calculated" : "-",

                                       PassportNumber = additional != null ? additional.PasportNo : "-",
                                       PassportExpiryDate = additional != null ? additional.PasportExpireDate : null,
                                       Nationality = e.Country != null ? e.Country.CountryName : "-",
                                       Religion = e.Religion != null ? e.Religion.ReligionName : "-",
                                       MaritalStatus = e.MaritalStatus != null ? e.MaritalStatus.MaritalStatusName : "-",

                                       // Supervisor details
                                       SupervisorName = supervisor != null ? supervisor.FirstName + " " + supervisor.LastName : "-",
                                       SupervisorImage = imgURL +  supervisor.EmployeeImageFileName 
                                   })
                   .FirstOrDefaultAsync();

                // Post-process the experience calculation (since EF can't translate DateOnly arithmetic)
                if (query != null && query.JoinDate.HasValue && query.Experience == "calculated")
                {
                    var joinDate = query.JoinDate.Value;
                    var currentDate = DateOnly.FromDateTime(DateTime.Now);
                    var totalDays = currentDate.DayNumber - joinDate.DayNumber;
                    var years = Math.Round(totalDays / 365.25, 1);
                    query.Experience = years + " years";
                }

                return  new CommonReturnViewModel
                    {
                        Success = true,
                        Data = query,
                        Message = "Employee basic details retrieved successfully."
                    };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };

            }
            

        }
    }
}
