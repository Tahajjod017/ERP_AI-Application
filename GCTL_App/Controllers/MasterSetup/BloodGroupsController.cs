using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.BloodGroups;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.BloodGroup;
using GCTL.Core.Helpers;

namespace GCTL_App.Controllers.MasterSetup
{
    public class BloodGroupsController : BaseController
    {
        #region Services & Repositories
        private readonly IBloodGroupService _bloodGroupService;

        public BloodGroupsController(ITranslateService translateService, IBloodGroupService bloodGroupService) : base(translateService)
        {
            _bloodGroupService = bloodGroupService;
        }
        #endregion


        #region Index
        //[Permission("View", "BloodGroups")]
        public IActionResult Index()
        {
            BloodGroupPageVM model = new BloodGroupPageVM();
            SetSmartPageCode(201000);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _bloodGroupService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "BloodGroupID", string sortOrder = "desc")
        {
            var result = await _bloodGroupService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Update
        //[Permission("Edit", "BloodGroups")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(BloodGroupVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _bloodGroupService.UpdateAsync(model);
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


        #region Create
        //[Permission("Create", "BloodGroups")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(BloodGroupVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _bloodGroupService.IsNameUniqueAsync(model.BloodGroupName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _bloodGroupService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.BloodGroupID });
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
                bool isUnique = await _bloodGroupService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "BloodGroups")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids  == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _bloodGroupService.SoftDeleteAsync(requestVM);
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
