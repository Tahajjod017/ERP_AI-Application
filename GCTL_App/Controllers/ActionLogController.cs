using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using System.Text.Json;
namespace GCTL_App.Controllers
{
    public class ActionLogController : BaseController
    {
        private readonly IActionLogService actionLogService;
        private readonly AppDbContext appDbContext;

        public ActionLogController(ITranslateService translateService, IUserProfileService userProfileService, IActionLogService actionLogService, AppDbContext appDbContext) : base(translateService, userProfileService)
        {
            this.actionLogService = actionLogService;
            this.appDbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var targetTypes = await appDbContext.ActionLogs
                    .Where(a => !string.IsNullOrEmpty(a.TargetType))
                    .Select(a => a.TargetType).Distinct().ToListAsync();

                var actionNames = await appDbContext.ActionLogs
                    .Where(a => !string.IsNullOrEmpty(a.ActionName) && a.ActionName != "Error Exception")
                    .Select(a => a.ActionName).Distinct().ToListAsync();

                var userNameEmail = await appDbContext.ActionLogs
                    .Include(x => x.CreatedByNavigation)
                    .Where(x => x.CreatedByNavigation != null).GroupBy(x => x.CreatedBy)
                    .Select(g => new
                    {
                        CreatedBy = g.Key,
                        UserEmail = g.Select(x => x.UserEmail).FirstOrDefault(),
                        FirstName = g.Select(x => x.CreatedByNavigation.FirstName).FirstOrDefault(),
                        LastName = g.Select(x => x.CreatedByNavigation.LastName).FirstOrDefault()
                    }).ToListAsync();

                //ViewBag.TargetTypeDD = new SelectList(targetTypes);
                //ViewBag.ActionNameDD = new SelectList(actionNames);
                ViewBag.TargetTypeDD = new SelectList(targetTypes.Select(x => new { Value = x, Text = x }), "Value", "Text");
                ViewBag.ActionNameDD = new SelectList(actionNames.Select(x => new { Value = x, Text = x }), "Value", "Text");



                ViewBag.UserNameDD = new SelectList(
                    userNameEmail.Select(x => new
                    {
                        Value = x.CreatedBy.ToString(),
                        Text = $"{x.FirstName} {x.LastName} ({x.UserEmail})"
                    }),
                    "Value", "Text"
                );
                SetSmartPageCode(3001000);
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View();
            }
        }


        #region Get With Pagination
        public async Task<IActionResult> ActionLogDataTable(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", DateTime? fromDate = null, DateTime? toDate = null, string? tergetType = null, string? actionName = null, int? createdBy = null)
        {
            // fromDate ??= DateTime.Today.AddDays(-2); // default FromDate = 2 days ago
            // toDate ??= DateTime.Today;
            var result = await actionLogService.GetPaginateActionLog(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, fromDate, toDate, tergetType, actionName, createdBy);
            return Json(result);


        }
        #endregion



        [HttpGet]
        public IActionResult GetActionLogDetails1(int actionLogId)
        {
            var log = appDbContext.ActionLogs.Include(x => x.CreatedByNavigation).FirstOrDefault(x => x.ActionLogID == actionLogId);

            if (log == null)
                return NotFound();

            Dictionary<string, object> actionBefore = new();
            Dictionary<string, object> actionAfter = new();

            try
            {
                if (!string.IsNullOrWhiteSpace(log.ActionBefore))
                {
                    var before = JsonSerializer.Deserialize<Dictionary<string, object>>(log.ActionBefore);
                    if (before != null)
                        actionBefore = before;
                }

                if (!string.IsNullOrWhiteSpace(log.ActionAfter))
                {
                    var after = JsonSerializer.Deserialize<Dictionary<string, object>>(log.ActionAfter);
                    if (after != null)
                        actionAfter = after;
                }
            }
            catch (JsonException ex)
            {
                return BadRequest($"JSON Deserialization error: {ex.Message}");
            }

            // Collect all keys from both dictionaries
            var allKeys = actionBefore.Keys.Union(actionAfter.Keys).Distinct();

            // Filtered dictionaries
            var filteredBefore = new Dictionary<string, object>();
            var filteredAfter = new Dictionary<string, object>();

            foreach (var key in allKeys)
            {
                actionBefore.TryGetValue(key, out var beforeValue);
                actionAfter.TryGetValue(key, out var afterValue);

                // Skip if both before and after are null/empty/whitespace
                if ((beforeValue == null || string.IsNullOrWhiteSpace(beforeValue.ToString())) &&
                    (afterValue == null || string.IsNullOrWhiteSpace(afterValue.ToString())))
                {
                    continue;
                }

                // Add non-empty values
                if (beforeValue != null) filteredBefore[key] = beforeValue;
                if (afterValue != null) filteredAfter[key] = afterValue;
            }

            return Ok(new
            {
                ActionBefore = filteredBefore,
                ActionAfter = filteredAfter,
                ActionLogID = log.ActionLogID,
                CreatedAt = log.CreatedAt,
                UserEmail = log.UserEmail,
                TargetType = log.TargetType,
                TargetID = log.TargetID,
                ActionName = log.ActionName,
                EmployeeID = log.CreatedBy,
                EmployeeUserName = log.CreatedByNavigation != null ? $"{log.CreatedByNavigation.FirstName} {log.CreatedByNavigation.LastName}" : "",
            });
        }




        [HttpGet]
        public IActionResult GetActionLogDetails(int actionLogId)
        {
            var log = appDbContext.ActionLogs.Include(x => x.CreatedByNavigation)
                .FirstOrDefault(x => x.ActionLogID == actionLogId);

            if (log == null)
                return NotFound();

           
            var actionBefore = new Dictionary<string, object>();
            var actionAfter = new Dictionary<string, object>();

            try
            {
                // Safely attempt to deserialize ActionBefore and ActionAfter only if they are not null or empty.
                if (!string.IsNullOrWhiteSpace(log.ActionBefore))
                {
                    var before = JsonSerializer.Deserialize<Dictionary<string, object>>(log.ActionBefore);
                    if (before != null)
                        actionBefore = before;
                }

                if (!string.IsNullOrWhiteSpace(log.ActionAfter))
                {
                    var after = JsonSerializer.Deserialize<Dictionary<string, object>>(log.ActionAfter);
                    if (after != null)
                        actionAfter = after;
                }
            }
            catch (JsonException ex)
            {
                return BadRequest($"Error during JSON deserialization: {ex.Message}");
            }


            var changedValues = new List<object>();


            foreach (var key in actionBefore.Keys)
            {

                if (actionAfter.TryGetValue(key, out var afterValue))
                {
                    var beforeValue = actionBefore[key];
                    if (!object.Equals(beforeValue?.ToString(), afterValue?.ToString()))
                    {
                        changedValues.Add(new
                        {
                            Field = key,
                            Before = beforeValue,
                            After = afterValue,

                        });
                    }
                }
            }

            return Ok(new
            {
                ActionLogID = log.ActionLogID,
                CreatedAt = log.CreatedAt,
                UserEmail = log.UserEmail,
                TargetType = log.TargetType,
                TargetID = log.TargetID,
                ActionName = log.ActionName,
                EmployeeID = log.CreatedBy ?? 0,
                EmployeeUserName = log.CreatedByNavigation != null ? $"{log.CreatedByNavigation.FirstName} {log.CreatedByNavigation.LastName}" : "",
                ChangedValues = changedValues
            });


          
        }



    }
}
