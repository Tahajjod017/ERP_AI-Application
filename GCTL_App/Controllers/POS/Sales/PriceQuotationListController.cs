using GCTL.Service.Language;
using GCTL.Service.POS.Sales.PriceQuotationList;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Sales
{
    public class PriceQuotationListController : BaseController
    {
        private readonly IPriceQuotationList _priceQuotationListService;

        public PriceQuotationListController(ITranslateService translateService, IUserProfileService userProfileService, IPriceQuotationList priceQuotationListService) : base(translateService, userProfileService)
        {
            _priceQuotationListService = priceQuotationListService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(90258800);

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetPriceQuotationList(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "CreatedAt", string sortDirection = "desc")
        {
            try
            {
                var result = await _priceQuotationListService.GetPriceQuotationsWithPagination(page, pageSize, searchTerm, sortColumn, sortDirection);
                return Json(new { success = true, data = result.Data, totalRecords = result.TotalRecords, totalPages = result.TotalPages, currentPage = page });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}
