using GCTL.Service.Language;
using GCTL.Service.POS.Sales.ShipmentList;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Sales.ShipmentF
{
    public class ChallanListController : BaseController
    {
        private readonly IChallanList _shipmentListService;

        public ChallanListController(ITranslateService translateService, IUserProfileService userProfileService, IChallanList shipmentListService) : base(translateService, userProfileService)
        {
            _shipmentListService = shipmentListService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(90260300);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetShipmentList(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "CreatedAt", string sortDirection = "desc")
        {
            try
            {
                var result = await _shipmentListService.GetChallanWithPagination(page, pageSize, searchTerm, sortColumn, sortDirection);

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
