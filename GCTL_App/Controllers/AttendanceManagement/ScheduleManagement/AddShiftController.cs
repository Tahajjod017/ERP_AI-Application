using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AddShift;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    [Authorize]
    public class AddShiftController : BaseController
    {
        #region Services & Repositories
        private readonly IAddShiftService _addShiftService;
        private readonly IGenericRepository<Shifts> _genericRepository;
        private readonly ICommonService _commonService;

        public AddShiftController(ITranslateService translateService,
            IUserProfileService userProfileService,
            IAddShiftService addShiftService,
            IGenericRepository<Shifts> genericRepository,
            ICommonService commonService) : base(translateService, userProfileService)
        {
            _addShiftService = addShiftService;
            _genericRepository = genericRepository;
            _commonService = commonService;
        }
        #endregion


        #region Index
        //[Permission("View", "Add Shift")]
        public IActionResult Index()
        {
            ShiftsPageVM model = new ShiftsPageVM();

            SetSmartPageCode(202800);

            ViewBag.OrganizationDD = new SelectList(_addShiftService.GetOrganizations(), "Id", "Name");

            return View(model);
        }
        #endregion


        #region SearchOrganizations
        [HttpGet]
        public async Task<IActionResult> SearchOrganizations(string search, int page = 1, int pageSize = 50)
        {
            var result = await _commonService.SearchOrganizations(search, page, pageSize);

            return Json(new
            {
                items = result.Items.Select(x => new {
                    value = x.Id,
                    label = x.Name,
                    group = x.GroupName 
                }),
                hasMore = result.HasMore
            });
        }
        #endregion


        #region Create
        //[Permission("Create", "Add Shift")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(ShiftsSetupVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    foreach (var orgId in model.OrganizationIDs)
                    {
                        if (orgId > 0)
                        {
                            var uniqueName = await _addShiftService.IsNameUniqueAsync(orgId, model.ShiftName);
                            if (!uniqueName)
                            {
                                return Json(new { isSuccess = false, message = $"{model.ShiftName} Shift is already exists!" });
                            }
                        }
                    }
                    // userInfoService.SetUserInfo(model, User, HttpContext);

                    await _addShiftService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ShiftID });
                }

                var orderedKeys = new[] { "ShiftName", "OrganizationIDs" };

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


        #region Update
        //[Permission("Edit", "Add Shift")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(ShiftUpdateSetupVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // userInfoService.SetUserInfo(model, User, HttpContext);
                    await _addShiftService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully." });
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


        #region CheckNameUnique
        [HttpPost]
        public async Task<IActionResult> CheckNameUnique(int id, string name)
        {
            try
            {
                bool isUnique = await _addShiftService.IsNameUniqueAsync(id, name);
                if (!isUnique)
                {
                    var orgId = await _genericRepository.All().Include(x => x.Organization).FirstOrDefaultAsync(x => x.OrganizationID == id);

                    string orgName = orgId.Organization.OrganizationName;

                    return Json($"{name} shift is already exists for {orgName ?? "this company"}.");
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json("Error occurred: " + ex.Message);
            }
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _addShiftService.GetByIdAsync(id);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No data found!" });
                }

                return Json(new { isSuccess = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ShiftID", string sortOrder = "desc", int? organizationID = null)
        {
            var result = await _addShiftService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);

            return Json(result);
        }
        #endregion


        #region Delete
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No bank selected to delete." });
                }

                var result = await _addShiftService.SoftDeleteAsync(requestVM);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No banks found to delete." });
                }

                return Json(new { isSuccess = true, message = "Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion
    }
}
