using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Sales.ShipmentList
{
    public class ShipmentListDto
    {
        public int ShipmentID { get; set; }
        public string ShipmentNumber { get; set; }
        public string SourceType { get; set; }
        public string SourceNumber { get; set; }
        public DateTime? ShipmentDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public decimal? ShippingCost { get; set; }
        public int TotalItems { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string ShippingAddress { get; set; }
    }

    public class ShiPaginatedResultData<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
