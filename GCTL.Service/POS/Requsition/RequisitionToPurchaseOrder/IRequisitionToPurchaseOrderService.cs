using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Core.ViewModels.POS.Requsition.RequisitionToPurchaseOrder;

namespace GCTL.Service.POS.Requsition.RequisitionToPurchaseOrder
{
    public interface IRequisitionToPurchaseOrderService
    {
        Task<PaginatedResultCommon<ApprovedRequisitionItemViewModel>> GetApprovedRequisitionsAsync(
            int? orgId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? productTypeId, string? fromDate, string? toDate);

        Task<RequisitionDetailsForPOViewModel> GetRequisitionDetailsForPOAsync(int requisitionId);

        Task<CommonReturnViewModel> ConvertToPurchaseOrderAsync(
            ConvertToPurchaseOrderViewModel model, int? empId, int? org, BaseViewModel? baseView);

        Task<string> GetNextPOCodeAsync();

        Task<byte[]> GeneratePDF(int orgId, string? fromDate, string? toDate);

        Task<byte[]> GenerateExcel(int orgId, string? fromDate, string? toDate);
    }
}
