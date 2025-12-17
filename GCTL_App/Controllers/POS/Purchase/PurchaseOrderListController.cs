using GCTL.Service.Language;
using GCTL.Service.POS.Purchase.PurchaseOrderList;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GCTL_App.Controllers.POS.Purchase
{
    public class PurchaseOrderListController : BaseController
    {
        private readonly IPurchaseOrderList _purchaseOrderListService;

        public PurchaseOrderListController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IPurchaseOrderList purchaseOrderListService)
            : base(translateService, userProfileService)
        {
            _purchaseOrderListService = purchaseOrderListService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(90259400);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrderList(
            int page = 1,
            int pageSize = 10,
            string searchTerm = "",
            string sortColumn = "CreatedAt",
            string sortDirection = "desc")
        {
            try
            {
                var result = await _purchaseOrderListService.GetPurchaseOrdersWithPagination(
                    page, pageSize, searchTerm, sortColumn, sortDirection);

                return Json(new
                {
                    success = true,
                    data = result.Data,
                    totalRecords = result.TotalRecords,
                    totalPages = result.TotalPages,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
