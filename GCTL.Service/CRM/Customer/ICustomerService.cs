using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.Customer;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.CRM.Customer
{
    public interface ICustomerService
    {
        Task<ReturnView> CreateCustomer(CustomerVM customerVM);
        Task<ReturnView> UpdateCustomer(CustomerVM model);
        Task<ReturnView> CreateBranch(BranchVM branchVM);
        Task<ReturnView> CreateWarehouse(WarehouseVM model);
        Task<ReturnView> CreateShipping(ShippingVM model);
        Task<PaginationService<Customers, CustomerTableDataVM>.PaginationResult<CustomerTableDataVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CustomerName", string sortOrder = "asc");
        Task<CustomerVM> GetCustomerInfo(int id, int organizationID);
        Task<BranchVM> GetBranchInfo(int customerID, int branchId, int organizationID);
        Task<List<BranchVM>> GetBranchList(int companyID, int organizationID);
    }
}
