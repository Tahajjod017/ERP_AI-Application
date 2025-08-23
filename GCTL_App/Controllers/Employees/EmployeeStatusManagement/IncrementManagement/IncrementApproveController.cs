using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeStatus.Increment;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.IncrementManagement
{
    public class IncrementApproveController : BaseController
    {
        private readonly IincrementService _incrementService;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<EmployeeActionTypes> _actionRepository;
        private readonly IGenericRepository<EmployeeCareerChangeHistory> _careerHistoryRepository;


        public IncrementApproveController(ITranslateService translateService, IUserProfileService userProfileService, IincrementService incrementService, IGenericRepository<Statuses> statusRepository, IGenericRepository<EmployeeActionTypes> actionRepository, IGenericRepository<EmployeeCareerChangeHistory> careerHistoryRepository)
            : base(translateService, userProfileService)
        {
            _incrementService = incrementService;
            _statusRepository = statusRepository;
            _actionRepository = actionRepository;
            _careerHistoryRepository = careerHistoryRepository;
        }



        public IActionResult Index()
        {
            ViewBag.IncrementTypeList = new SelectList(
                _actionRepository.AllActive().Where(r => r.EmployeeActionTypeName.ToLower() == "increment" || r.EmployeeActionTypeName.ToLower() == "decrement")
                .Select(d => new { d.EmployeeActionTypeID, d.EmployeeActionTypeName }),
                "EmployeeActionTypeID",
                "EmployeeActionTypeName"
            );

            SetSmartPageCode(121900);
            return View();
        }

        [HttpGet]
        public IActionResult GetDetails(int id)
        {

            var app = _careerHistoryRepository.AllActive()
             .Include(e => e.ApprovalPerson)
             .Include(s => s.Status)
             .Where(e => e.EmployeeCareerChangeID == id)
             .ToList() // Forces client-side evaluation
             .Select((w, index) => new
             {
                 approverStep = index,
                 statusName = w.Status != null ? w.Status.StatusName : "",
                 approvarPerson = w.ApprovalPerson.FirstName + " " + w.ApprovalPerson.LastName,
                 approvarNote = w.Remarks != null ? w.Remarks : "no remarks",
                 approvedOrDeclineDate = w.CreatedAt.Value.ToString("dd/MM/yyyy hh:mm tt")

             })
             .ToList();

           



            return Json(app);
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

   
}
