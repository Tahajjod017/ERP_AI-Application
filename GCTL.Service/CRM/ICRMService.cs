
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;


namespace GCTL.Service.CRM
{
    public interface ICRMService
    {

        Task<(List<LeadsTableVM> Leads, int TotalCount)> GetLeads(
            int currentOrgId,
            int customerType,
            string dateRange,
            string leadStatus2,
            int pageNumber,
            int pageSize,
            string searchTerm,
            string sortColumn,
            string sortDirection
        );
        public Task<PaginatedResult<CommonSelectVM>> SearchOrganizations(string search, int page = 1, int pageSize = 50);

    }
}
