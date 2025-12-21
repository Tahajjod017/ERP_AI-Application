using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Core.ViewModels.POS.Requsition.RequisitionApprover;

namespace GCTL.Service.POS.Requsition.RequisitionApprover
{
    public interface IRequisitionApproverService
    {
        Task<PaginatedResultCommon<RequisitionApprovalItemViewModel>> GetPendingApprovalsAsync(
            int? empId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? productTypeId, string? fromDate, string? toDate);

        Task<PaginatedResultCommon<RequisitionApprovalItemViewModel>> GetApprovedHistoryAsync(
            int? empId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? productTypeId, string? fromDate, string? toDate);

        Task<RequisitionDetailsViewModel> GetRequisitionDetailsAsync(int requisitionId, int? empId);

        Task<CommonReturnViewModel> ApproveRequisitionAsync(
            ApproveRequisitionViewModel model, int? empId, BaseViewModel? baseView);

        Task<CommonReturnViewModel> DeclineRequisitionAsync(
            DeclineRequisitionViewModel model, int? empId, BaseViewModel? baseView);

        Task<CommonReturnViewModel> EditApprovedRequisitionAsync(
            EditApprovedRequisitionViewModel model, int? empId, BaseViewModel? baseView);

        Task<byte[]> GeneratePDF(int orgId, int empId, string? fromDate, string? toDate, bool approved);

        Task<byte[]> GenerateExcel(int orgId, int empId, string? fromDate, string? toDate, bool approved);
    }
}