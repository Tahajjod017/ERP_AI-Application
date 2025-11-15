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
        #region service & repository
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
        #endregion

        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "GenderName", string sortOrder = "asc")
        {
            var result = await _customerService.GetAllAsync(await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region GetWarehouseList
        public async Task<IActionResult> GetWarehouseList(int customerID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "WarehouseName", string sortOrder = "asc")
        {
            var result = await _customerService.GetAllWarehouseAsync(customerID, await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region GetShippingList
        public async Task<IActionResult> GetShippingList(int customerID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "WarehouseName", string sortOrder = "asc")
        {
            var result = await _customerService.GetAllShippingAsync(customerID, await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region SaveCustomer
        [HttpPost]
        public async Task<IActionResult> SaveCustomer([FromBody] CustomerVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.Id == 0)
                {
                    result = await _customerService.CreateCustomer(model);
                }
                else
                {
                    result = await _customerService.UpdateCustomer(model);
                }
                   
                return Ok(result);
            }
            catch (Exception ex) {

                return Json(new { success = false, message = ex });
            }
            

        }
        #endregion

        #region SaveBranch
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
                else
                    result = await _customerService.UpdateBranch(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = ex });
            }
        }
        #endregion

        #region SaveShipping
        [HttpPost]
        public async Task<IActionResult> SaveShipping([FromBody] ShippingVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.Sid == 0)
                    result = await _customerService.CreateShipping(model);
                else
                    result = await _customerService.UpdateShipping(model);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = ex });
            }
        }
        #endregion

        #region SaveWarehouse
        [HttpPost]
        public async Task<IActionResult> SaveWarehouse([FromBody] WarehouseVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data" });
                var result = new ReturnView();
                if (model.Wid == 0)
                    result = await _customerService.CreateWarehouse(model);
                else
                    result = await _customerService.UpdateWarehouse(model);
                return  Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex) {

                return Json(new { success = false, message = ex });
            }
        }
        #endregion

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

        #region GetWarehouseInfo
        [HttpPost]
        public async Task<IActionResult> GetWarehouseInfo(int customerID, int warehouseId)
        {
            try
            {
                if (warehouseId <= 0 || customerID <= 0)
                    return Json(new { success = false, message = "Id is not accessible" });
                var result = await _customerService.GetWarehouseInfo(customerID, warehouseId, await GetCurrentOrganizationIdAsync() ?? 0);
                return Ok(result);
            }
            catch (Exception ex) {
                return Json(new { success = false, messsage = ex });
            }
        }

        #endregion

        #region GetShippingInfo
        [HttpPost]
        public async Task<IActionResult> GetShippingInfo(int customerID, int shippingId)
        {
            try
            {
                if (shippingId <= 0 || customerID <= 0)
                    return Json(new { success = false, message = "Id is not accessible" });
                var result = await _customerService.GetShippingInfo(customerID, shippingId, await GetCurrentOrganizationIdAsync() ?? 0);
                return Ok(result);
            }
            catch (Exception ex) {
                return Json(new { success = false, messsage = ex });
            }
        }

        #endregion

        #region GetBranchList
        [HttpGet]
        public async Task<IActionResult> GetBranchList(int customerID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "WarehouseName", string sortOrder = "asc")
        {
            try
            {
                if (customerID <= 0)
                    return Json(new { success = false, message = "Id is not accessible" });
                var result = await _customerService.GetAllBranchAsync(customerID, await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
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

        #region get BranchTypesList
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

        #region delete Contact person
        [HttpPost]
        public async Task<IActionResult> DeleteContactPerson(int contactId)
        {
            try
            {
                var result = await _customerService.DeleteContactPersonAsync(contactId);
                return Ok(result);
            } catch (Exception)
            {
                return Ok(new { success = false });
            }
        }
        #endregion

        #region get Branch List for dropdown
        [HttpGet]
        public async Task<IActionResult> GetBranches(int customerID, string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _customerService.GetPagedBranchesAsync(
                customerID, search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );

            var more = (page * pageSize) < result.totalItem;

            var formatted = new
            {
                results = result.data != null ?  result.data.Select(c => new
                {
                    id = c.Bid,
                    text = $"{c.BName}"
                }) : [],
                pagination = new { more }
            };

            return Ok(formatted);
        }
        #endregion
    }
}
