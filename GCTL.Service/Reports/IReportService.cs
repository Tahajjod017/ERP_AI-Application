using GCTL.Core.ViewModels.RequisitionDistribution.Purchase.CreatePO;

namespace GCTL.Service.ReportService
{
    public interface IReportService
    {
        Task<byte[]> GeneratePOPdfAsync(PurchaseOrderSubmissionViewModel model);
    }
}