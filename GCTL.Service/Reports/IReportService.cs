using GCTL.Core.ViewModels.POS.Purchasess.Purchase.CreatePO;

namespace GCTL.Service.ReportService
{
    public interface IReportService
    {
        Task<byte[]> GeneratePOPdfAsync(PurchaseOrderSubmissionViewModel model);
    }
}