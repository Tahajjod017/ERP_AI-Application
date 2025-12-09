using GCTL.Service.Language;
using GCTL.Service.POS.Sales.InvoiceListF;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Sales.InvoiceF
{
    public class InvoiceListController : BaseController
    {
        private readonly IInvoiceList _invoiceListService;

        public InvoiceListController(ITranslateService translateService, IUserProfileService userProfileService, IInvoiceList invoiceListService)
            : base(translateService, userProfileService)
        {
            _invoiceListService = invoiceListService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(902400);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceList(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "CreatedAt", string sortDirection = "desc")
        {
            try
            {
                var result = await _invoiceListService.GetInvoicesWithPagination(page, pageSize, searchTerm, sortColumn, sortDirection);

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
