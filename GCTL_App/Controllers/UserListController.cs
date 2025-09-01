using GCTL.Core.ViewModels.Login;
using GCTL.Data.Models;
using GCTL.Service;
using GCTL.Service.AccessPermissions;
using GCTL.Service.Language;
using GCTL.Service.Pagination;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Linq.Expressions;

namespace GCTL_NBR.Controllers
{
    [Authorize]
    public class UserListController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;
        private readonly IAccessControlService _accessControlService;

        public UserListController(ITranslateService translateService, IUserProfileService userProfileService, UserManager<ApplicationUser> userManager, AppDbContext db, IAccessControlService accessControlService) : base(translateService, userProfileService)
        {
            _userManager = userManager;
            _db = db;
            _accessControlService = accessControlService;
        }

        [Permission("VIEW", "UserList")]
        public async Task<IActionResult> Index()
        {
            ViewBag.CanExportPermission = await _accessControlService.HasPermissionAsync(4038, "Export");
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetPaginatedUsers(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmployeeCode", string sortOrder = "asc")
        {
            var excludedCodes = new[] { "DEV001", "DEV002", "DEV003", "DEV004", "DEV005" };

            var query = _userManager.Users
                                    .Include(u => u.Employees)
                                    .Where(u => u.Employees.EmployeeID != null &&
                                     !excludedCodes.Contains(u.Employees.EmployeeCode)&& u.Employees.IsActive==true) 
                                    .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Employees.EmployeeCode.Contains(searchTerm) || ((u.Employees.FirstName ?? "") + " " + (u.Employees.LastName ?? "")).Contains(searchTerm));
            }

            // Define the search predicate
            Func<string, Expression<Func<ApplicationUser, bool>>> searchPredicate = term =>
                u => u.Employees.EmployeeCode.Contains(term) || ((u.Employees.FirstName ?? "") + " " + (u.Employees.LastName ?? "")).Trim().Contains(term);

            // Call PaginationService to fetch paginated data
            var paginatedResult = await PaginationService<ApplicationUser, UserListVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                searchPredicate,
                u => new UserListVM
                {
                    Id = u.Id,
                    EmployeeCode = u.Employees.EmployeeCode ?? "-",
                    EmployeeName = (u.Employees.FirstName +" "+u.Employees.LastName).Trim() ?? "-",
                    //IsActive =  u.DeletedAt == null ? "Active" : "Inactive",
                    //UserName = u.UserName ?? "-",
                    //Email = u.Email ?? "-",
                    // Role = "-", // You can extend this later
                    //Status = u.LockoutEnabled ? "Inactive" : "Active",
                });

            // Return the data and pagination info as JSON
            return Json(new
            {
                data = paginatedResult.Data,
                paginationInfo = paginatedResult.PaginationInfo
            });
        }
    }
}
