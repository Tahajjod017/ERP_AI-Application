using GCTL.Core.ViewModels.CRM;

namespace GCTL.Service.CRM.LeadsActivities
{
    public interface ILeadsActivityService
    {
        Task<ReturnDataView<LeadDetailsDTO>> GetUpcomingActivityList(int page, int itemPerPage, string search, string sort, string direction, string dateRange, int? userID, int? CustomerTypeID, string? LeadStatusID, int? ActivityTypeID);
        Task<bool> GenerateAndSendEmployeePDFsAsync(string wwwRootPath);
    }
}
