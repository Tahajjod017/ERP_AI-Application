using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.ActionTakens;
using GCTL_App.ViewModels.MasterSetup.ActionTakens;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class ActionTakensController : Controller
    {
        #region Services & Repositories
        private readonly IActionTakenService _actionTakenService;
        public ActionTakensController(IActionTakenService actionTakenService)
        {
            _actionTakenService = actionTakenService;
        }
        #endregion


        #region Index
        public IActionResult Index()
        {
            ActionTakenPageVM model = new ActionTakenPageVM();
            
            return View(model);
        }
        #endregion


        #region Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(ActionTakenVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   // userInfoService.SetUserInfo(model, User, HttpContext);
                    var uniqueName = await _actionTakenService.IsNameUniqueAsync(model.ActionTakenName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }

                    await _actionTakenService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ActionTakenID });
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
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(ActionTakenVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   // userInfoService.SetUserInfo(model, User, HttpContext);
                    await _actionTakenService.UpdateAsync(model);
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
                bool isUnique = await _actionTakenService.IsNameUniqueAsync(name);
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


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _actionTakenService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ActionTakenID", string sortOrder = "desc")
        {
            var result = await _actionTakenService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

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
               
                var result = await _actionTakenService.SoftDeleteAsync(requestVM);
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
