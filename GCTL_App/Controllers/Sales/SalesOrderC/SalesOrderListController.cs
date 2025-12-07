using GCTL.Service.Language;
using GCTL.Service.Sales.SalesOrderList;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Sales.SalesOrderC
{
    public class SalesOrderListController : BaseController
    {
        private readonly ISalesOrderList _salesOrderListService;

        public SalesOrderListController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            ISalesOrderList salesOrderListService)
            : base(translateService, userProfileService)
        {
            _salesOrderListService = salesOrderListService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesOrderList(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "CreatedAt", string sortDirection = "desc")
        {
            try
            {
                var result = await _salesOrderListService.GetSalesOrdersWithPagination(page, pageSize, searchTerm, sortColumn, sortDirection);

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
