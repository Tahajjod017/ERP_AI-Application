using GCTL.Service.Language;
using GCTL.Service.POS.Sales.ShipmentList;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Sales.ShipmentF
{
    public class ShipmentListController : BaseController
    {
        private readonly IShipmentList _shipmentListService;

        public ShipmentListController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IShipmentList shipmentListService)
            : base(translateService, userProfileService)
        {
            _shipmentListService = shipmentListService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(90260300);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetShipmentList(
            int page = 1,
            int pageSize = 10,
            string searchTerm = "",
            string sortColumn = "CreatedAt",
            string sortDirection = "desc")
        {
            try
            {
                var result = await _shipmentListService.GetShipmentsWithPagination(
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
