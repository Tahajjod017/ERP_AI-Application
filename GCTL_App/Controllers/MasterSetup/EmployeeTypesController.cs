using GCTL.Core.ViewModels.MasterSetup.EmployeeType;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.EmployeeTypes;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.EmployeeType;
using GCTL.Core.Helpers;

namespace GCTL_App.Controllers.MasterSetup
{
    public class EmployeeTypesController : BaseController
    {
        #region Services & Repositories
        private readonly IEmployeeTypesService _employeeTypesService;
        private readonly ITranslateService _translationService;


        public EmployeeTypesController(IEmployeeTypesService employeeTypesService, ITranslateService translationService, ITranslateService translateService) : base(translateService)
        {
            _employeeTypesService = employeeTypesService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "EmployeeTypes")]
        public IActionResult Index()
        {
            EmployeeTypesPageVM model = new EmployeeTypesPageVM();
            SetSmartPageCode(209000);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _employeeTypesService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmployeeTypeName", string sortOrder = "asc")
        {
            var result = await _employeeTypesService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "EmployeeTypes")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeTypesVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _employeeTypesService.IsNameUniqueAsync(model.EmployeeTypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _employeeTypesService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.EmployeeTypeID });
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
        //[Permission("Edit", "EmployeeTypes")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(EmployeeTypesVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _employeeTypesService.UpdateAsync(model);
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
                bool isUnique = await _employeeTypesService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "EmployeeTypes")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _employeeTypesService.SoftDeleteAsync(requestVM);
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
