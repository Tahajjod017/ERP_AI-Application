
using GCTL.Core.ViewModels.Employee.EmployeeListVM;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeList;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeListController : BaseController
    {
        private readonly IEmployeeListService _employeeListService;

        public EmployeeListController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeListService employeeListService) : base(translateService, userProfileService)
        {
            _employeeListService = employeeListService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] int page = 1, [FromQuery] int limit = 3, 
            [FromQuery] string department = "", [FromQuery] string status = "", [FromQuery] string sort = "", 
            [FromQuery] string search = "")
        {
            try
            {
                // Build query
                IQueryable<EmployeeListGetViewModel> query = await _employeeListService.GetEmployees();

                // Filter by department
                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(e => e.Department == department);
                }

                // Filter by status
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(e => e.Status == status);
                }

                // Search by name or email
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(e =>
                        e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
                }



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

                query = query.OrderByDescending(e => e.JoiningDate);




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
                        name = e.Name,
                        email = e.Email,
                        phone = e.Phone,
                        department = e.Department,
                        joiningDate = e.JoiningDate,
                        status = e.Status,
                        avatar = e.Avatar ?? "../../../assets/img/team/72x72/58.webp"
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
    }
}
