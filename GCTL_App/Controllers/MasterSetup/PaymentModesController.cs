using GCTL.Core.ViewModels.MasterSetup.PaymentModes;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.PaymentMode;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.PaymentModes;
using GCTL.Core.Helpers;

namespace GCTL_App.Controllers.MasterSetup
{
    public class PaymentModesController : BaseController
    {
        #region Services & Repositories
        private readonly IPaymentModeService _paymentModeService;
        private readonly ITranslateService _translationService;


        public PaymentModesController(IPaymentModeService paymentModeService, ITranslateService translationService, ITranslateService translateService) : base(translateService)
        {
            _paymentModeService = paymentModeService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "PaymentModes")]
        public IActionResult Index()
        {
            PaymentModePageVM model = new PaymentModePageVM();
            SetSmartPageCode(201800);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _paymentModeService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PaymentModeID", string sortOrder = "desc")
        {
            var result = await _paymentModeService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "PaymentModes")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(PaymentModeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _paymentModeService.IsNameUniqueAsync(model.PaymentModeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _paymentModeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.PaymentModeID });
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
        //[Permission("Edit", "PaymentModes")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(PaymentModeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _paymentModeService.UpdateAsync(model);
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
                bool isUnique = await _paymentModeService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "PaymentModes")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _paymentModeService.SoftDeleteAsync(requestVM);
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
