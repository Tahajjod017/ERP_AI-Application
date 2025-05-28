using GCTL.Core.ViewModels.MasterSetup.Departments;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Department;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.Departments;
using GCTL.Core.Helpers;
using GCTL.Service.UserProfile;

namespace GCTL_App.Controllers.MasterSetup
{
    public class DepartmentsController : BaseController
    {
        #region Services & Repositories
        private readonly IDepartmentService _departmentService;
        private readonly ITranslateService _translationService;


        public DepartmentsController(IDepartmentService departmentService, ITranslateService translationService, ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            _departmentService = departmentService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "Departments")]
        public IActionResult Index()
        {
            DepartmentPageVM model = new DepartmentPageVM();
            SetSmartPageCode(205000);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _departmentService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DepartmentID", string sortOrder = "desc")
        {
            var result = await _departmentService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "Departments")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(DepartmentVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _departmentService.IsNameUniqueAsync(model.DepartmentName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _departmentService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.DepartmentID });
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
        //[Permission("Edit", "Departments")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(DepartmentVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _departmentService.UpdateAsync(model);
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
                bool isUnique = await _departmentService.IsNameUniqueAsync(bankName);
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
        [Permission("Delete", "Departments")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _departmentService.SoftDeleteAsync(requestVM);
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
