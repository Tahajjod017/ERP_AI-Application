using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeOfficialController : BaseController
    {

        #region CTOR

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeOfficeInfo> _employeeOfficialRepository;
        private readonly IGenericRepository<GCTL.Data.Models.OrganizationBranches> _branchRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<EmployeeType> _employeeTypeRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<EmploymentNature> _employmentNatureRepository;
        private readonly IGenericRepository<Statuses> _employeeStatusRepository;
        private readonly IEmployeeOfficialService _employeeOfficialService;


        public EmployeeOfficialController(ITranslateService translateService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<OrganizationBranches> branchRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<EmployeeType> employeeTypeRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmploymentNature> employmentNatureRepository, IGenericRepository<Statuses> employeeStatusRepository, IEmployeeOfficialService employeeOfficialService, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository) : base(translateService)
        {
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _organizationRepository = organizationRepository;
            _employeeTypeRepository = employeeTypeRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _employmentNatureRepository = employmentNatureRepository;
            _employeeStatusRepository = employeeStatusRepository;
            _employeeOfficialService = employeeOfficialService;
            _employeeOfficialRepository = employeeOfficialRepository;
        }

        #endregion


        #region Index 
        public IActionResult Index(int id)
        {
            #region ViewBagData

            ViewBag.EmployeeDD = new SelectList(
                _employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.OrganizationDD = new SelectList(
                _organizationRepository.All().Select(o => new { o.OrganizationID, o.OrganizationName }),
                "OrganizationID",
                "OrganizationName"
            );

            ViewBag.BranchDD = new SelectList(
                _branchRepository.All().Select(b => new { b.OrganizationBranchID, b.OrganizationBranchName }),
                "OrganizationBranchID",
                "OrganizationBranchName"
            );

            ViewBag.EmployeeTypeDD = new SelectList(
                _employeeTypeRepository.All().Select(et => new { et.EmployeeTypeID, et.EmployeeTypeName }),
                "EmployeeTypeID",
                "EmployeeTypeName"
            );

            ViewBag.DepartmentDD = new SelectList(
                _departmentRepository.All().Select(d => new { d.DepartmentID, d.DepartmentName }),
                "DepartmentID",
                "DepartmentName"
            );

            ViewBag.DesignationDD = new SelectList(
                _designationRepository.All().Select(d => new { d.DesignationID, d.DesignationName }),
                "DesignationID",
                "DesignationName"
            );

            ViewBag.EmploymentNatureDD = new SelectList(
                _employmentNatureRepository.All().Select(en => new { en.EmploymentNatureID, en.EmploymentNatureName }),
                "EmploymentNatureID",
                "EmploymentNatureName"
            );

            ViewBag.SeniorSupervisorDD = new SelectList(
                _employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.ImmediateSupervisorDD = new SelectList(
                _employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.HeadOfDepartmentDD = new SelectList(
                _employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.EmployeeStatusDD = new SelectList(
                _employeeStatusRepository.All().Select(es => new { es.StatusID, es.StatusName }),
                "StatusID",
                "StatusName"
            );


            // ViewBag.TimeUnitDD = new SelectList(_timeUnitRepository.All().Select(tu => new { tu.TimeUnitID, tu.TimeUnitName }), "TimeUnitID", "TimeUnitName");


            ViewBag.TimeUnitDD = new SelectList(new List<object>{
                new { TimeUnitID = 1, TimeUnitName = "Days" },
                new { TimeUnitID = 2, TimeUnitName = "Months" },
                new { TimeUnitID = 3, TimeUnitName = "Years" }
            }, "TimeUnitID", "TimeUnitName");
            #endregion

            EmployeeOfficialPostViewModel model = GetEmployeeDetailsMethod(id);
       
            SetSmartPageCode(112000);
            return View(model);
        }


        [HttpPost]
        public async Task< IActionResult> Index(EmployeeOfficialPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var chkDuplicate = await _employeeOfficialService.CheckValidEmployeeInfo(model);

                if (!chkDuplicate.Success)
                {
                    return Ok(chkDuplicate);
                }


                if (model.EmployeeOfficeInfoID == 0 || model.EmployeeOfficeInfoID == null)
                {
                    var result = await _employeeOfficialService.SaveEmployeeOfficialInfo(model);

                    if (!result.Success)
                    {
                        return Ok(result);
                    }

                    return Ok(result);
                }
                else
                {
                    var result = await _employeeOfficialService.UpdateEmployeeOfficialInfo(model);

                    if (!result.Success)
                    {
                        return Ok(result);
                    }

                    return Ok(result);
                }

                
            }

            return Ok(new { success = false, message = "ModelState Is Not Valid!" });
           
        }


        #endregion

        #region GetEmployeeDetails

        [HttpGet]
        public IActionResult GetEmployeeDetails(int id)
        {
           var model =  GetEmployeeDetailsMethod(id);

            return Ok(model);
        }


        
        private EmployeeOfficialPostViewModel GetEmployeeDetailsMethod(int id)
        {
            var empPersonal = _employeeRepository.AllActive().FirstOrDefault(e => e.EmployeeID == id);
            var empOfficial = _employeeOfficialRepository.AllActive().FirstOrDefault(e => e.EmployeeID == id);

            EmployeeOfficialPostViewModel model = new EmployeeOfficialPostViewModel();

            if (empPersonal != null)
            {
                model.EmployeePersonalId = empPersonal.EmployeeID;
                model.PersonalEmail = empPersonal.Email;
                model.PersonalPhone = empPersonal.MobileNumber;
            }

            if (empOfficial != null)
            {
                model.EmployeeOfficeId = empOfficial.EmployeeOfficeId ?? string.Empty;
                model.EmployeeOfficeInfoID = empOfficial.EmployeeOfficeInfoID;
                model.OrganizationID = empOfficial.OrganizationID;
                model.OrganizationBranchID = empOfficial.OrganizationBranchID;
                model.DepartmentID = empOfficial.DepartmentID;
                model.DesignationID = empOfficial.DesignationID;
                model.EmployeeTypeID = empOfficial.EmployeeTypeID;
                model.EmploymentNatureID = empOfficial.EmploymentNatureID;
                model.SeniorSupervisorId = empOfficial.SeniorSupervisorId;
                model.ImmediateSupervisorId = empOfficial.ImmediateSupervisorId;
                model.HeadOfDepartmentId = empOfficial.HeadOfDepartmentId;
                model.OfficePhone = empOfficial.OfficePhone ?? string.Empty;
                model.OfficeEmail = empOfficial.OfficeEmail ?? string.Empty;
                model.AttendanceId = empOfficial.AttendanceId ?? string.Empty;
                model.EmploymentStatusId = empOfficial.EmploymentStatusId;
                model.AppointmentLetterNo = empOfficial.AppointmentLetterNo ?? string.Empty;
                model.AppointmentLetterIssueDate = empOfficial.AppointmentLetterIssueDate;
                model.JoiningDate = empOfficial.JoiningDate;
                model.ProvisionPeriodStartDate = empOfficial.ProvisionPeriodStartDate;
                model.ProvisionPeriod = empOfficial.ProvisionPeriod;
                model.ProvisionPeriodTtimeTypeID = empOfficial.ProvisionPeriodTtimeTypeID;
                model.ConfirmationDate = empOfficial.ConfirmationDate;
                model.ConfirmationLetterNo = empOfficial.ConfirmationLetterNo ?? string.Empty;
                model.ContractEndDate = empOfficial.ContractEndDate;
            }

            return model;
        }

        #endregion
    }
}
