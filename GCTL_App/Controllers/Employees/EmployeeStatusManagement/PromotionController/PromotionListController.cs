using System.Linq;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeStatus.Promotion;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.PromotionController
{
    public class PromotionListController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOffiRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<EmployeeCareerChanges> _empCarrerRepository;
        private readonly IGenericRepository<EmployeeActionTypes> _empActionRepository;
        private readonly IPromotionService _promotionService;

        public PromotionListController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmployeeCareerChanges> empCarrerRepository, IPromotionService promotionService, IGenericRepository<EmployeeActionTypes> empActionRepository) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeOffiRepository = employeeOffiRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _empCarrerRepository = empCarrerRepository;
            _promotionService = promotionService;
            _empActionRepository = empActionRepository;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(112100);


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

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GetPromotionList(PromotionListFilterViewModel filters)
        {
            try
            {
                var imgLink = GetEmployeePictureURL(true);

                var matches = new[] { "promotion", "demotion" };

                var proDemoIDs = _empActionRepository.AllActive()
                    .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                    .Select(x => x.EmployeeActionTypeID)
                    .ToList();


                var query = _empCarrerRepository.AllActive().Include(x => x.Employee)
                    .ThenInclude(navigationPropertyPath => navigationPropertyPath.EmployeeOfficeInfoEmployee)
                    .ThenInclude(navigationPropertyPath => navigationPropertyPath.Department)
                    
                    .Where(x => proDemoIDs.Contains((int)x.EmployeeActionTypeID));  

                // Search term filter (employee name, maybe others)
                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    query = query.Where(x => x.Employee.FirstName.Contains(filters.SearchTerm));
                }

                // Department filter
                if (filters.DepartmentId.HasValue)
                {
                    query = query.Where(x =>    x.Employee.EmployeeOfficeInfoEmployee.Any(ofc => ofc.DepartmentID == filters.DepartmentId.Value));

                }

                // Status filter
                if (!string.IsNullOrEmpty(filters.Status))
                {
                    query = query.Where(x => x.Status.StatusName == filters.Status);
                }

                // Date range filter
                if (!string.IsNullOrEmpty(filters.DateRange))
                {
                    var a = DateTime.TryParse(filters.DateRange, out DateTime startDate);

                     query = query.Where(x => x.EffectiveDate == startDate );


                   
                }

                // Total record count before pagination
                var totalRecords = await query.CountAsync();

                // Sorting
                if (!string.IsNullOrEmpty(filters.SortColumn))
                {
                    query = filters.SortColumn switch
                    {
                        "employeeName" => filters.SortDirection == "desc"
                            ? query.OrderByDescending(x => x.Employee.FirstName)
                            : query.OrderBy(x => x.Employee.FirstName),

                        //"department" => filters.SortDirection == "desc"
                        //    ? query.OrderByDescending(x => x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.Department.DepartmentName)
                        //    : query.OrderBy(x => x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.Department.DepartmentName),

                        "department" => filters.SortDirection == "desc"
                                ? query.OrderByDescending(x => x.Employee.EmployeeOfficeInfoEmployee.OrderBy(ofc => ofc.EmployeeOfficeInfoID)
                                    .Select(ofc => ofc.Department.DepartmentName).FirstOrDefault())
                                : query.OrderBy(x => x.Employee.EmployeeOfficeInfoEmployee.OrderBy(ofc => ofc.EmployeeOfficeInfoID)
                                    .Select(ofc => ofc.Department.DepartmentName).FirstOrDefault()),

                        "effectiveDate" => filters.SortDirection == "desc"
                            ? query.OrderByDescending(x => x.EffectiveDate)
                            : query.OrderBy(x => x.EffectiveDate),

                        _ => query.OrderBy(x => x.EmployeeCareerChangeID) // default
                    };
                }

                // Pagination
                var skip = (filters.PageNumber - 1) * filters.PageSize;
                var items = await query.Skip(skip).Take(filters.PageSize)
                    .Select(x => new
                    {
                        id = x.EmployeeCareerChangeID,
                        employeeName = x.Employee.FirstName + " " + x.Employee.LastName,
                        department = x.Employee.EmployeeOfficeInfoEmployee.OrderBy(ofc => ofc.EmployeeOfficeInfoID).Select(ofc => ofc.Department.DepartmentName).FirstOrDefault(),
                        currentDesignation = x.CurentDesignation.DesignationName,
                        newDesignation = x.NewDesignation.DesignationName,
                        effectiveDate = x.EffectiveDate.Value.ToString("dd-MM-yyyy"),
                        salaryChange = $"{x.CurrentSalary ?? 0:#,0} → {x.NewSalary ?? 0:#,0}",
                        status = x.Status.StatusName,
                        avatarUrl = imgLink + x.Employee.EmployeeImageFileName
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        items,
                        pagination = new
                        {
                            currentPage = filters.PageNumber,
                            totalPages = (int)Math.Ceiling((double)totalRecords / filters.PageSize),
                            totalRecords,
                            startRecord = skip + 1,
                            endRecord = skip + items.Count
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}
