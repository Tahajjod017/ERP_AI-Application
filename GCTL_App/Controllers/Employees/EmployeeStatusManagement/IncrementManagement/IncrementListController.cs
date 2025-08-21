using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment;
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
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<EmployeeCareerChanges> _empCarrerRepository;
        public IncrementListController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmployeeCareerChanges> empCarrerRepository, IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository, IGenericRepository<Statuses> statusRepository) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _empCarrerRepository = empCarrerRepository;
            _employeeOffiRepository = employeeOffiRepository;
            _statusRepository = statusRepository;
        }


        [HttpGet]
        public IActionResult GetDetails(int id)
        {
            // Create dummy approval steps data in timeline format
            var approvalSteps = new List<object>
            {
                new {
                    approverStep = "1",
                    statusName = "APPROVED",
                    approvarPerson = "Joe Biden",
                    approvarNote = "No Note",
                    approvedOrDeclineDate = "06-08-2025 4:49 PM"
                },
                new {
                    approverStep = "2",
                    statusName = "APPROVED",
                    approvarPerson = "Jeff Bezos",
                    approvarNote = "No Note",
                    approvedOrDeclineDate = "06-08-2025 4:51 PM"
                },
                //new {
                //    approverStep = "3",
                //    statusName = "DECLINED",
                //    approvarPerson = "Md Shefain",
                //    approvarNote = "No note",
                //    approvedOrDeclineDate = "06-08-2025 4:55 PM"
                //}
            };

           

            return Json(approvalSteps);
        }

        public IActionResult Index()
        {
            SetSmartPageCode(111800);

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new
            {
                id = e.EmployeeID,
                name = e.FirstName + " " + e.LastName
            }), "id", "name");

            ViewBag.OrganizationDD = new SelectList(
               _organizationRepository.AllActive().Select(o => new { o.OrganizationID, o.OrganizationName }),
               "OrganizationID",
               "OrganizationName"
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

            ViewBag.IncrementTypeList = new SelectList(
                _statusRepository.AllActive().Where(r=>r.StatusType == "AppDecPen").Select(d => new { d.StatusID, d.StatusName }),
                "StatusID",
                "StatusName"
            );

            return View();
        }

        #region Get ALL For Table


        [HttpPost]
        public IActionResult GetIncrementList(string searchTerm, int? departmentId, string incrementType, string dateRange, int pageSize = 10, int pageNumber = 1, string sortColumn = "effectiveDate", string sortDirection = "desc")
        {
            var imgLink = GetEmployeePictureURL(true);

            // Create a DTO class or use an existing one to avoid anonymous types in the query
            var query = from ecc in _empCarrerRepository.AllActive().Where(x => x.EmployeeID != null)
                        join emp in _employeeRepository.AllActive() on ecc.EmployeeID.Value equals emp.EmployeeID
                        join off in _employeeOffiRepository.AllActive() on emp.EmployeeID equals off.EmployeeID
                        join dept in _departmentRepository.AllActive() on off.DepartmentID equals dept.DepartmentID
                        select new IncrementListItem
                        {
                            EmployeeCareerChangeID = ecc.EmployeeCareerChangeID,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            DepartmentName = dept.DepartmentName,
                            DepartmentID = (int)off.DepartmentID,
                            CurrentSalary = ecc.CurrentSalary,
                            NewSalary = ecc.NewSalary,
                            EffectiveDate = ecc.EffectiveDate,
                            EmployeeActionTypeID = (int)ecc.EmployeeActionTypeID,
                            StatusName = ecc.Status != null ? ecc.Status.StatusName : "N/A",
                            StatusId = ecc.Status != null ? ecc.Status.StatusID : 0,
                            EmployeeImageFileName = emp.EmployeeImageFileName
                        };

            // 🔍 Filter: Search by employee name
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(searchTerm));
            }

            // 📂 Filter: Department
            if (departmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentID == departmentId.Value);
            }

            // 🧾 Filter: Increment/Decrement Type
            if (!string.IsNullOrEmpty(incrementType))
            {
                var type = Convert.ToInt16(incrementType); // ✅ convert the string, not the query
                query = query.Where(x => x.StatusId == type);
            }

            // 📅 Filter: Date Range
            if (!string.IsNullOrWhiteSpace(dateRange))
            {
                var dates = dateRange.Split(" to ");
                if (dates.Length == 2 &&
                    DateTime.TryParse(dates[0], out var startDate) &&
                    DateTime.TryParse(dates[1], out var endDate))
                {
                    query = query.Where(x => x.EffectiveDate >= startDate && x.EffectiveDate <= endDate);
                }
            }

            // 🔀 Apply sorting - Now using strongly typed approach
            IOrderedQueryable<IncrementListItem> orderedQuery;

            switch (sortColumn?.ToLower())
            {
                case "employeename":
                    orderedQuery = sortDirection == "asc"
                        ? query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                        : query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName);
                    break;

                case "department":
                    orderedQuery = sortDirection == "asc"
                        ? query.OrderBy(x => x.DepartmentName)
                        : query.OrderByDescending(x => x.DepartmentName);
                    break;

                case "currentsalary":
                    orderedQuery = sortDirection == "asc"
                        ? query.OrderBy(x => x.CurrentSalary)
                        : query.OrderByDescending(x => x.CurrentSalary);
                    break;

                case "newsalary":
                    orderedQuery = sortDirection == "asc"
                        ? query.OrderBy(x => x.NewSalary)
                        : query.OrderByDescending(x => x.NewSalary);
                    break;

                case "incrementamount":
                    orderedQuery = sortDirection == "asc"
                        ? query.OrderBy(x => (x.NewSalary ?? 0) - (x.CurrentSalary ?? 0))
                        : query.OrderByDescending(x => (x.NewSalary ?? 0) - (x.CurrentSalary ?? 0));
                    break;

                case "incrementtype":
                    orderedQuery = sortDirection == "asc"
                        ? query.OrderBy(x => x.EmployeeActionTypeID)
                        : query.OrderByDescending(x => x.EmployeeActionTypeID);
                    break;

                case "status":
                    orderedQuery = sortDirection == "asc"
                        ? query.OrderBy(x => x.StatusName)
                        : query.OrderByDescending(x => x.StatusName);
                    break;

                case "effectivedate":
                default:
                    orderedQuery = sortDirection == "asc"
                        ? query.OrderBy(x => x.EffectiveDate)
                        : query.OrderByDescending(x => x.EffectiveDate);
                    break;
            }

            // 📊 Get total count AFTER filtering but BEFORE pagination
            var totalRecords = orderedQuery.Count();

            // 📄 Apply pagination and select final data
            var data = orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    id = x.EmployeeCareerChangeID,
                    employeeName = x.FirstName + " " + x.LastName,
                    department = x.DepartmentName,
                    currentSalary = x.CurrentSalary,
                    incrementAmount = (x.NewSalary ?? 0) - (x.CurrentSalary ?? 0),
                    newSalary = x.NewSalary,
                    effectiveDate = x.EffectiveDate.HasValue ? x.EffectiveDate.Value.ToString("dd-MM-yyyy") : "N/A",
                    incrementType = x.EmployeeActionTypeID == 1 ? "Increment" : "Decrement",
                    status = x.StatusName,
                    avatarUrl = !string.IsNullOrEmpty(x.EmployeeImageFileName) ? imgLink + x.EmployeeImageFileName : "/images/default-avatar.png"
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
                        startRecord = totalRecords > 0 ? ((pageNumber - 1) * pageSize) + 1 : 0,
                        endRecord = Math.Min(pageNumber * pageSize, totalRecords)
                    }
                }
            });
        }




        #endregion


    }

   
}
