using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;

namespace GCTL.Service.CRM.LeadCreate
{
    public interface ILeadCreateService
    {
        #region CRUD
        Task<CommonReturnViewModel> CreateLead(int orgId, LeadsVM leadsVM);
        Task<CommonReturnViewModel> EditLead(LeadsVM leadUpdateVM);
        Task<ReturnDataView<CommonSelectVM>> GetLeadSourceListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CommonSelectVM>> GetLeadStatusListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CommonSelectVM>> GetPriorityListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CustomerInfoVM>> GetLeadOwnerListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CommonSelectVM>> GetServiceListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CommonSelectVM2>> GetContactPersonNumberAsync(int leadId, string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CommonSelectVM2>> GetContactPersonEmailAsync(int leadId, string search, int page, int pageSize, int organizationID);
        #endregion
    }
}
