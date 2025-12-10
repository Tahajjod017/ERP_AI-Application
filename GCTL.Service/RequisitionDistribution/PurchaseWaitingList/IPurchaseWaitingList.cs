using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.RequisitionDistribution.Purchase.PurchaseWaitionList;

namespace GCTL.Service.RequisitionDistribution.PurchaseWaitingList
{
    public interface IPurchaseWaitingList
    {
        PurchaseWaitingListViewModel GetPurchaseWaitingList(FilterOptions filters, PaginationInfo pagination);
        List<PurchaseRequestItem> GetAllPurchaseRequests();
        //PurchaseRequestItem GetPurchaseRequestById(string reqId);

        PurchaseRequestViewModel GetPurchaseRequestById(string reqId);
        Task<byte[]> GeneratePDF(int orgid, int id);

    }
}
