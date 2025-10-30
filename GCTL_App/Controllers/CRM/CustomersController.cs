using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.Customer;
using GCTL.Service.CRM.Customer;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GCTL_App.Controllers.CRM
{
    public class CustomersController : BaseController
    {
        private readonly ICustomerService _customerService;
        public CustomersController(ITranslateService translateService, IUserProfileService userProfileService, ICustomerService customerService) : base(translateService, userProfileService)
        {
            _customerService = customerService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult IndexModal()
        {
            return PartialView("_indexModal");
        }

        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "GenderName", string sortOrder = "asc")
        {
            var result = await _customerService.GetAllAsync(await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        [HttpPost]
        public async Task<IActionResult> SaveCustomer([FromBody] CustomerVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.ID == 0)
                    result = await _customerService.CreateCustomer(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = "Invalid data" });
            }
            

        }

        [HttpPost]
        public async Task<IActionResult> SaveBranch([FromBody] BranchVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.BID == 0)
                    result = await _customerService.CreateBranch(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = "Invalid data" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveShipping([FromBody] ShippingVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.SID == 0)
                    result = await _customerService.CreateShipping(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = "Invalid data" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveWarehouse([FromBody] WarehouseVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.WID == 0)
                    result = await _customerService.CreateWarehouse(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = "Invalid data" });
            }
        }


    }
}
