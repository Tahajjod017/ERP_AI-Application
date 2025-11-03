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
                if (model.Id == 0)
                    result = await _customerService.CreateCustomer(model);
                else 
                    result = await _customerService.UpdateCustomer(model);
                    return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = ex });
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
                if (model.Bid == 0)
                    result = await _customerService.CreateBranch(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = ex });
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

                return Json(new { success = false, message = ex });
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

                return Json(new { success = false, message = ex });
            }
        }


        #region getCustomerInfo
        [HttpPost]
        public async Task<IActionResult> GetCustoerInfo(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "Id is not accessible" });
                var result = await _customerService.GetCustomerInfo(id, await GetCurrentOrganizationIdAsync() ?? 0);
                return Ok(result);
            }
            catch (Exception ex) {
                return Json(new { success = false, messsage = ex });
            }
        }

        #endregion
        #region GetBranchInfo
        [HttpPost]
        public async Task<IActionResult> GetBranchInfo(int customerID, int branchId)
        {
            try
            {
                if (branchId <= 0 || customerID <= 0)
                    return Json(new { success = false, message = "Id is not accessible" });
                var result = await _customerService.GetBranchInfo(customerID, branchId, await GetCurrentOrganizationIdAsync() ?? 0);
                return Ok(result);
            }
            catch (Exception ex) {
                return Json(new { success = false, messsage = ex });
            }
        }

        #endregion
        #region getCustomerInfo
        [HttpPost]
        public async Task<IActionResult> GetBranchList(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "Id is not accessible" });
                var result = await _customerService.GetBranchList(id, await GetCurrentOrganizationIdAsync() ?? 0);
                return Ok(result);
            }
            catch (Exception ex) {
                return Json(new { success = false, messsage = ex });
            }
        }

        #endregion

        #region get OrganizationTypesList
        [HttpGet]
        public async Task<IActionResult> GetOrganizationTypesList(string search = "", int page = 1, int pageSize = 10)
        {
            var result = await _customerService.GetOrganizationTypesList(
                search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0, "Organization"
            );

            var more = (page * pageSize) < result.totalItem;

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.Value,
                    text = c.Text
                }),
                pagination = new { more }
            };

            return Ok(formatted);
        }
        #endregion
        #region get OrganizationTypesList
        [HttpGet]
        public async Task<IActionResult> GetBranchTypesList(string search = "", int page = 1, int pageSize = 10)
        {
            var result = await _customerService.GetOrganizationTypesList(
                search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0, "Branch"
            );

            var more = (page * pageSize) < result.totalItem;

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.Value,
                    text = c.Text
                }),
                pagination = new { more }
            };

            return Ok(formatted);
        }
        #endregion
    }
}
