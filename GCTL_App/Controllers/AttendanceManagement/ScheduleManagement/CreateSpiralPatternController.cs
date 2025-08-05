using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Service.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    public class CreateSpiralPatternController : BaseController
    {
        #region Services
        private readonly ICreateSpiralPatternService _createSpiralPatternService;
        private readonly ICommonService _commonService;


        public CreateSpiralPatternController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            ICreateSpiralPatternService createSpiralPatternService,
            ICommonService commonService) : base(translateService, userProfileService)
        {
            _createSpiralPatternService = createSpiralPatternService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index()
        {
            CreateSpiralPatternPageVM model = new CreateSpiralPatternPageVM();
            SetSmartPageCode(203300);

            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.BrnchDD = new SelectList(await _commonService.GetBranches(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await _commonService.GetDepartments(), "Id", "Name");
            ViewBag.ShiftDD = new SelectList(await _commonService.GetShifts(), "Id", "Name");
            ViewBag.CompensationDD = new SelectList(await _commonService.GetCompensation(), "Id", "Name");
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.SpiralPatternTypeDD = new SelectList(await _commonService.GetSpiralPatternTypes(), "Id", "Name");

            return View(model);
        }
        #endregion


        #region Create
        //[Permission("Create", "OffDayRoster")]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateSpiralPatternVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _createSpiralPatternService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                // Custom ordered validation message 
                var orderedKeys = new[] { "OrganizationID", "SpiralPatternTypeID", "SpiralPatternName" };

                foreach (var key in orderedKeys)
                {
                    if (ModelState.TryGetValue(key, out var entry) && entry.Errors.Any())
                    {
                        return Json(new { isSuccess = false, field = key, message = entry.Errors.First().ErrorMessage });
                    }
                }

                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetShiftByOrganization
        [HttpGet]
        public async Task<IActionResult> GetShiftByOrganization(int? id)
        {
            var result = await _commonService.GetShiftsByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetAllSpiralWeeklyPatternAsync
        [Route("/CreateSpiralPattern/GetAllSpiralWeeklyPatternAsync")]
        [HttpGet]
        public async Task<IActionResult> GetAllSpiralWeeklyPatternAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "SpiralWeeklyPatternID", string sortOrder = "desc")
        {
            try
            {
                var (data, pagination) = await _createSpiralPatternService.GetAllSpiralWeeklyPatternAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
                return Json(new
                {
                    isSuccess = true,
                    data,
                    pagination
                });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetAllSpiralFortnightlyPatternAsync
        [Route("/CreateSpiralPattern/GetAllSpiralFortnightlyPatternAsync")]
        [HttpGet]
        public async Task<IActionResult> GetAllSpiralFortnightlyPatternAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "SpiralBioWeeklyPatternID", string sortOrder = "desc")
        {
            try
            {
                var (data, pagination) = await _createSpiralPatternService.GetAllSpiralFortnightlyPatternAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
                return Json(new
                {
                    isSuccess = true,
                    data,
                    pagination
                });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetAllSpiralMonthlyPatternAsync
        [Route("/CreateSpiralPattern/GetAllSpiralMonthlyPatternAsync")]
        [HttpGet]
        public async Task<IActionResult> GetAllSpiralMonthlyPatternAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "SpiralMonthlyPatternID", string sortOrder = "desc")
        {
            try
            {
                var (data, pagination) = await _createSpiralPatternService.GetAllSpiralMonthlyPatternAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
                return Json(new
                {
                    isSuccess = true,
                    data,
                    pagination
                });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion
    }
}
