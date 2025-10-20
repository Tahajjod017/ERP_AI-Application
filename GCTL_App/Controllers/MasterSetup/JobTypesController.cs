using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.JobTypes;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.JobTypes;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.MasterSetup.Genders;
using GCTL_App.ViewModels.MasterSetup.JobTypes;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class JobTypesController : BaseController
    {
        #region Services & Repositories
        private readonly IJobTypeService _jobTypeService;
        private readonly ITranslateService _translationService;

        public JobTypesController(ITranslateService translateService, IUserProfileService userProfileService, IJobTypeService jobTypeService, ITranslateService translationService) : base(translateService, userProfileService)
        {
            _jobTypeService = jobTypeService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "Genders")]
        public IActionResult Index()
        {
            JobTypePageVM model = new JobTypePageVM();
            SetSmartPageCode(201200);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _jobTypeService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "JobTypeName", string sortOrder = "asc")
        {
            var result = await _jobTypeService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "Genders")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(JobTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _jobTypeService.IsNameUniqueAsync(model.JobTypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _jobTypeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.JobTypeID });
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
        //[Permission("Edit", "Genders")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(JobTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _jobTypeService.UpdateAsync(model);
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
        public async Task<IActionResult> CheckNameUnique(string name)
        {
            try
            {
                bool isUnique = await _jobTypeService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "Genders")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _jobTypeService.SoftDeleteAsync(requestVM);
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
