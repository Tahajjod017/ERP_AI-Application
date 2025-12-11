using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchasess.Purchase.PurchaseWaitionList
{
    public class PurchaseWaitingListViewModel
    {
        public List<PurchaseRequestItem> PurchaseRequests { get; set; } = new List<PurchaseRequestItem>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
        public FilterOptions Filters { get; set; } = new FilterOptions();
    }

    public class PurchaseRequestItem
    {
        public int PurchaseId { get; set; }

        public string PoId;

        public string ReqId { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public string RequestedBy { get; set; }
        public DateTime? RequestDate { get; set; }
        public string ProductType { get; set; }
        public int? ProductTypeId { get; set; }
        public string ProductName { get; set; }
        public int? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        
    }

    public class FilterOptions
    {
        public string SearchTerm { get; set; }
        public int? ProductTypeId { get; set; }
        public int? ProjectId { get; set; }
        public int? ProductId { get; set; }
        public string SortBy { get; set; } = "ReqId";
        public string SortDirection { get; set; } = "asc";
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    }


}
