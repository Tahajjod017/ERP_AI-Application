using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;

namespace GCTL.Service.CRM.LeadCreate
{
    public interface ILeadCreateService
    {
        #region CRUD
       // Task<bool> AddAsync(CreateLeadVM model);
        //Task<ReturnView> CreatePerson(CustomerVM customerVM);
        //Task<ReturnView> CreateCompany(CompanyVM companyVM);
        //Task<CommonReturnViewModel> CreateShippingAddress(ShippingVM shippingVM);
        Task<CommonReturnViewModel> CreateLead(LeadsVM leadsVM);
        Task<CommonReturnViewModel> EditLead(LeadUpdateVM leadUpdateVM);
        //Task<ReturnView> CreateBranch(BranchVM branchVM);
        //Task<ReturnView> CreateWarehouse(WarehouseVM warehouseVM);
        public Task<object?> getcustomerInfo(int? id);
        Task<ReturnDataView<CommonSelectVM>> GetLeadSourceListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CommonSelectVM>> GetLeadStatusListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CommonSelectVM>> GetPriorityListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CustomerInfoVM>> GetLeadOwnerListAsync(string search, int page, int pageSize, int organizationID);
        #endregion
    }
}
