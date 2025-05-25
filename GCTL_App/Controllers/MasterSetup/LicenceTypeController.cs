using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.LicenceType;
using GCTL.Service.MasterSetup.LicenceType;
using GCTL_App.ViewModels.MasterSetup.LicenceType;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class LicenceTypeController : Controller
    {
        #region Services & Repositories
        private readonly ILicenceTypeService _licenceTypeService;

        public LicenceTypeController(ILicenceTypeService licenceTypeService)
        {
            _licenceTypeService = licenceTypeService;
        }
        #endregion


        #region Index
        public IActionResult Index()
        {
            LicenceTypePageVM model = new LicenceTypePageVM();

            return View(model);
        }
        #endregion


        #region Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(LicenceTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // userInfoService.SetUserInfo(model, User, HttpContext);
                    var uniqueName = await _licenceTypeService.IsNameUniqueAsync(model.LicenceTypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }

                    await _licenceTypeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.LicenceTypeID });
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
        public async Task<IActionResult> Update(LicenceTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // userInfoService.SetUserInfo(model, User, HttpContext);
                    await _licenceTypeService.UpdateAsync(model);
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
                bool isUnique = await _licenceTypeService.IsNameUniqueAsync(name);
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
                var result = await _licenceTypeService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "LicenceTypeID", string sortOrder = "desc")
        {
            var result = await _licenceTypeService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

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

                var result = await _licenceTypeService.SoftDeleteAsync(requestVM);
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
