using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.Customer;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.Service.CRM.Customer
{
    public interface ICustomerService
    {
        Task<ReturnView> CreateCustomer(CustomerVM customerVM);
        Task<ReturnView> UpdateCustomer(CustomerVM model);
        Task<ReturnView> CreateBranch(BranchVM branchVM);
        Task<ReturnView> UpdateBranch(BranchVM branchVM);
        Task<ReturnView> CreateWarehouse(WarehouseVM model);
        Task<ReturnView> UpdateWarehouse(WarehouseVM model);
        Task<ReturnView> CreateShipping(ShippingVM model);
        Task<ReturnView> UpdateShipping(ShippingVM model);
        Task<ShippingVM> GetShippingInfo(int customerID, int shippingId, int organizationID);
        Task<PaginationService<Customers, CustomerTableDataVM>.PaginationResult<CustomerTableDataVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CustomerName", string sortOrder = "asc");
        Task<CustomerVM> GetCustomerInfo(int id, int organizationID);
        Task<BranchVM> GetBranchInfo(int customerID, int branchId, int organizationID);
        Task<ReturnView> CreateOrUpdateContactInfo(List<ClintContact> model, int addressId);
        Task<WarehouseVM> GetWarehouseInfo(int customerID, int branchId, int organizationID);
        Task<PaginationService<CompanyBranches, BranchVM>.PaginationResult<BranchVM>>
         GetAllBranchAsync(
             int companyID,
             int organizationID,
             int pageNumber = 1,
             int pageSize = 5,
             string searchTerm = "",
             string sortColumn = "BranchName",
             string sortOrder = "asc");
        Task<ReturnDataView<SelectListItem>> GetOrganizationTypesList(string search, int page, int pageSize, int organizationID, string userFor = "Branch");
        Task<PaginationService<CompanyWarehouses, WarehouseVM>.PaginationResult<WarehouseVM>> GetAllWarehouseAsync(int customerID, int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CreateAt", string sortOrder = "desc");
        Task<PaginationService<CustomerAddresses, ShippingVM>.PaginationResult<ShippingVM>> GetAllShippingAsync(int customerID, int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CreatedAt", string sortOrder = "desc");
    }
}
