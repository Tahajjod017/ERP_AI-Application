using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.IncrementManagement
{
    public class IncrementListController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOffiRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<EmployeeCareerChanges> _empCarrerRepository;
        public IncrementListController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmployeeCareerChanges> empCarrerRepository, IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _empCarrerRepository = empCarrerRepository;
            _employeeOffiRepository = employeeOffiRepository;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(111800);

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new
            {
                id = e.EmployeeID,
                name = e.FirstName + " " + e.LastName
            }), "id", "name");

            ViewBag.OrganizationDD = new SelectList(
               _organizationRepository.All().Select(o => new { o.OrganizationID, o.OrganizationName }),
               "OrganizationID",
               "OrganizationName"
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

            return View();
        }

        #region Get ALL For Table

       

        [HttpPost]
        public IActionResult GetIncrementList(string searchTerm, int? departmentId, string incrementType, string dateRange, int pageSize = 10, int pageNumber = 1, string sortColumn = "effectiveDate", string sortDirection = "desc" )
        {
            // Base query with necessary joins
            var query = from ecc in _empCarrerRepository.AllActive().Where(x => x.EmployeeID != null)
                        join emp in _employeeRepository.AllActive() on ecc.EmployeeID.Value equals emp.EmployeeID
                        join off in _employeeOffiRepository.AllActive() on emp.EmployeeID equals off.EmployeeID
                        join dept in _departmentRepository.AllActive() on off.DepartmentID equals dept.DepartmentID
                        select new
                        {
                            ecc,
                            emp,
                            off,
                            dept
                        };


            // 🔍 Filter: Search by employee name
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    (x.emp.FirstName + " " + x.emp.LastName).Contains(searchTerm));
            }

            // 📂 Filter: Department
            if (departmentId.HasValue)
            {
                query = query.Where(x => x.off.DepartmentID == departmentId.Value);
            }

            // 🧾 Filter: Increment/Decrement Type
            if (!string.IsNullOrEmpty(incrementType))
            {
                var type = incrementType.ToLower();
                query = query.Where(x =>
                    type == "increment" ? x.ecc.EmployeeActionTypeID == 1 :
                    type == "decrement" ? x.ecc.EmployeeActionTypeID == 2 : true);
            }

            // 📅 Filter: Date Range
            if (!string.IsNullOrWhiteSpace(dateRange))
            {
                var dates = dateRange.Split(" to ");
                if (dates.Length == 2 &&
                    DateTime.TryParse(dates[0], out var startDate) &&
                    DateTime.TryParse(dates[1], out var endDate))
                {
                    query = query.Where(x => x.ecc.EffectiveDate >= startDate && x.ecc.EffectiveDate <= endDate);
                }
            }

            // 🔀 Apply sorting
            switch (sortColumn?.ToLower())
            {
                case "employeename":
                    query = sortDirection == "asc"
                        ? query.OrderBy(x => x.emp.FirstName).ThenBy(x => x.emp.LastName)
                        : query.OrderByDescending(x => x.emp.FirstName).ThenByDescending(x => x.emp.LastName);
                    break;

                case "department":
                    query = sortDirection == "asc"
                        ? query.OrderBy(x => x.dept.DepartmentName)
                        : query.OrderByDescending(x => x.dept.DepartmentName);
                    break;

                case "currentsalary":
                    query = sortDirection == "asc"
                        ? query.OrderBy(x => x.ecc.CurrentSalary)
                        : query.OrderByDescending(x => x.ecc.CurrentSalary);
                    break;

                case "newsalary":
                    query = sortDirection == "asc"
                        ? query.OrderBy(x => x.ecc.NewSalary)
                        : query.OrderByDescending(x => x.ecc.NewSalary);
                    break;

                case "incrementamount":
                    query = sortDirection == "asc"
                        ? query.OrderBy(x => (x.ecc.NewSalary ?? 0) - (x.ecc.CurrentSalary ?? 0))
                        : query.OrderByDescending(x => (x.ecc.NewSalary ?? 0) - (x.ecc.CurrentSalary ?? 0));
                    break;

                case "incrementtype":
                    query = sortDirection == "asc"
                        ? query.OrderBy(x => x.ecc.EmployeeActionTypeID)
                        : query.OrderByDescending(x => x.ecc.EmployeeActionTypeID);
                    break;

                case "status":
                    query = sortDirection == "asc"
                        ? query.OrderBy(x => x.ecc.Status.StatusName)
                        : query.OrderByDescending(x => x.ecc.Status.StatusName);
                    break;

                default:
                    query = sortDirection == "asc"
                        ? query.OrderBy(x => x.ecc.EffectiveDate)
                        : query.OrderByDescending(x => x.ecc.EffectiveDate);
                    break;
            }



            // 📊 Pagination values
            var totalRecords = query.Count();

            var data = query
                .OrderByDescending(x => x.ecc.EffectiveDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    employeeName = x.emp.FirstName + " " + x.emp.LastName,
                    department = x.dept.DepartmentName,
                    currentSalary = x.ecc.CurrentSalary,
                    incrementAmount = (x.ecc.NewSalary ?? 0) - (x.ecc.CurrentSalary ?? 0),
                    newSalary = x.ecc.NewSalary,
                    effectiveDate = x.ecc.EffectiveDate.Value.ToString("dd-MM-yyyy"),
                    incrementType = x.ecc.EmployeeActionTypeID == 1 ? "Increment" : "Decrement",
                    status = x.ecc.Status != null ? x.ecc.Status.StatusName : "N/A"
                })
                .ToList();

            return Json(new
            {
                success = true,
                data = new
                {
                    items = data,
                    pagination = new
                    {
                        totalRecords,
                        currentPage = pageNumber,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                        startRecord = ((pageNumber - 1) * pageSize) + 1,
                        endRecord = Math.Min(pageNumber * pageSize, totalRecords)
                    }
                }
            });
        }



        #endregion
    }
}
