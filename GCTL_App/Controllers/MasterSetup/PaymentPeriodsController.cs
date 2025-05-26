using GCTL.Core.ViewModels.MasterSetup.PaymenPeriodTypes;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.PaymenPeriodType;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.PaymenPeriodTypes;
using GCTL.Core.Helpers;

namespace GCTL_App.Controllers.MasterSetup
{
    public class PaymentPeriodsController : BaseController
    {
        #region Services & Repositories
        private readonly IPaymentPeriodsService _paymentPeriodsService;
        private readonly ITranslateService _translationService;


        public PaymentPeriodsController(IPaymentPeriodsService paymentPeriodsService, ITranslateService translationService, ITranslateService translateService) : base(translateService)
        {
            _paymentPeriodsService = paymentPeriodsService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "PaymentPeriods")]
        public IActionResult Index()
        {
            PaymentPeriodPageVM model = new PaymentPeriodPageVM();
            SetSmartPageCode(201900);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _paymentPeriodsService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PaymenPeriodTypeName", string sortOrder = "asc")
        {
            var result = await _paymentPeriodsService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "PaymentPeriods")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(PaymentPeriodsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _paymentPeriodsService.IsNameUniqueAsync(model.PaymentPeriodTypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _paymentPeriodsService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.PaymentPeriodTypeID });
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
        //[Permission("Edit", "PaymentPeriods")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(PaymentPeriodsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _paymentPeriodsService.UpdateAsync(model);
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
                bool isUnique = await _paymentPeriodsService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "PaymentPeriods")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _paymentPeriodsService.SoftDeleteAsync(requestVM);
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
