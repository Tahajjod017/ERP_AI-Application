using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment;
using GCTL.Service.Employees.EmployeeStatus.Increment;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.IncrementManagement
{
    public class IncrementApproveController : BaseController
    {
        private readonly IincrementService _incrementService;

        public IncrementApproveController( ITranslateService translateService, IUserProfileService userProfileService, IincrementService incrementService)
            : base(translateService, userProfileService)
        {
            _incrementService = incrementService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(121900);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetIncrementCards()
        {
            // Mock data for demonstration; replace with actual logic
            var data = new
            {
                annualCount = 125,
                annualPending = 15,
                performanceCount = 85,
                performancePending = 8,
                promotionCount = 42,
                promotionPending = 12,
                specialCount = 28,
                specialPending = 5
            };
            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> GetPendingIncrements([FromForm] IncrementFilterModel filter)
        {
            var imgLink = GetEmployeePictureURL(true); // Get image URL for thumbnails
            var loggedID = GetCurrentEmployeeIdAsync().Result;
            var result = await _incrementService.GetFilteredIncrementsAsync(filter, imgLink, loggedID);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetApprovedIncrements([FromForm] IncrementFilterModel filter)
        {
            var imgLink = GetEmployeePictureURL(true); // Get image URL for thumbnails
            var loggedID = GetCurrentEmployeeIdAsync().Result;
            var result = await _incrementService.GetFilteredApprovedIncrementsAsync(filter, imgLink, loggedID);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetIncrementDetails(int id)
        {
            var increment = await _incrementService.GetPendingIncrementDetailsByID(id);
            if (increment == null)
            {
                return NotFound();
            }
            return Json(increment);
        }

        [HttpPost]
        public async Task<IActionResult> PerformIncrementAction([FromForm] IncrementActionModel model)
        {
            var result = await _incrementService.ApproveIncrementAsync(model);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ExportIncrements([FromForm] string format)
        {
            // Implement export logic (PDF/Excel) here
            return Ok(); // Placeholder
        }
    }

    //public class IncrementApproveController : BaseController
    //{
    //    public IncrementApproveController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
    //    {
    //    }

    //    public IActionResult Index()
    //    {
    //        SetSmartPageCode(121900);

    //        return View();
    //    }
    //}
}
