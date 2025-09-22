using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.LeadActivityType;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.LeadActivityType;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.MasterSetup.LeadActivityType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.MasterSetup
{
    public class LeadActivityTypesController : BaseController
    {
        #region Services & Repositories
        private readonly ILeadActivityTypeService _leadActivityTypeService;
        private readonly ITranslateService _translationService;


        public LeadActivityTypesController(ITranslateService translationService, ITranslateService translateService, IUserProfileService userProfileService, ILeadActivityTypeService leadActivityTypeService) : base(translateService, userProfileService)
        {
            _translationService = translationService;
            _leadActivityTypeService = leadActivityTypeService;
        }
        #endregion


        #region Index
        //[Permission("View", "Grades")]
        public IActionResult Index()
        {
            LeadActivityTypePageVM model = new LeadActivityTypePageVM();
            SetSmartPageCode(201300);
            var activityTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "General", Text="General"},
                new SelectListItem { Value = "Won", Text="Won"},
                new SelectListItem { Value = "Lost", Text="Lost"},
            };
            ViewBag.ActivityTypes = activityTypes;
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _leadActivityTypeService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "LeadActivityName", string sortOrder = "asc")
        {
            var result = await _leadActivityTypeService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "Grades")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(LeadActivityTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _leadActivityTypeService.IsNameUniqueAsync(model.LeadActivityName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _leadActivityTypeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.LeadActivityTypeID });
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
        //[Permission("Edit", "Grades")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(LeadActivityTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _leadActivityTypeService.UpdateAsync(model);
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
        public async Task<IActionResult> CheckNameUnique(string bankName)
        {
            try
            {
                bool isUnique = await _leadActivityTypeService.IsNameUniqueAsync(bankName);
                if (!isUnique)
                {
                    return Json("This name already exists.");
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json("Error occurred: " + ex.Message);
            }
        }
        #endregion


        #region SoftDelete
        [Permission("Delete", "Grades")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _leadActivityTypeService.SoftDeleteAsync(requestVM);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No id found to delete." });
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
