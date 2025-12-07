using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Sales.PriceQuotationList
{
    public class PriceQuotationListDto
    {
        public int PriceQuotationID { get; set; }
        public string QuotationNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime? QuotationDate { get; set; }
        public decimal? VatPercentage { get; set; }
        public int TotalItems { get; set; }
        public string CreatedBy { get; set; }
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
