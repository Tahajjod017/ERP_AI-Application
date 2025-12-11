using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchasess.Purchase.PurchaseWaitionList
{
    public class PurchaseRequestModel
    {
        public string? SearchTerm { get; set; }
        public int? ProductTypeId { get; set; }
        public int? ProjectId { get; set; }
        public int? ProductId { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
    }

}
