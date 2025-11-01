using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.Customer;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.CRM.Customer
{
    public interface ICustomerService
    {
        Task<ReturnView> CreateCustomer(CustomerVM customerVM);
        Task<ReturnView> CreateBranch(BranchVM branchVM);
        Task<ReturnView> CreateWarehouse(WarehouseVM model);
        Task<ReturnView> CreateShipping(ShippingVM model);
        Task<PaginationService<Customers, CustomerVM>.PaginationResult<CustomerVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CustomerName", string sortOrder = "asc");
    }
}
