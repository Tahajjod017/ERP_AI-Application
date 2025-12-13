using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchase.PurchaseOrderList
{
    public class PurchaseOrderListDto
    {
        public int PurchaseOrderID { get; set; }
        public string POID { get; set; }
        public string SupplierName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? GrandTotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? DueAmount { get; set; }
        public int TotalItems { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }

    public class PaginatedResultData<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
