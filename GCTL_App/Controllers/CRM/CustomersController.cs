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
        public async Task<IActionResult> SaveBranch([FromBody] AddressVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.ID == 0)
                    result = await _customerService.CreateBranch(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = "Invalid data" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveShipping([FromBody] AddressVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.ID == 0)
                    result = await _customerService.CreateBranch(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = "Invalid data" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveWarehouse([FromBody] AddressVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.ID == 0)
                    result = await _customerService.CreateBranch(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = "Invalid data" });
            }
        }

    }
}
