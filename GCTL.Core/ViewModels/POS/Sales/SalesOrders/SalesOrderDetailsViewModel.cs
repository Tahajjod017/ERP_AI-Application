using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationDetails;

namespace GCTL.Core.ViewModels.POS.Sales.SalesOrders
{
    public class SalesOrderDetailsViewModel : BaseViewModel
    {
        public int? Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderNumber { get; set; }
        public int? SelectedCustomerId { get; set; }
        public int? SelectedQuotationId { get; set; }
        public string QuotationNumber { get; set; }
        public decimal VatPercent { get; set; }
        public string Note { get; set; }
        public List<SalesOrderItemDetails> Items { get; set; } = new List<SalesOrderItemDetails>();

        // Calculated fields
        public decimal SubTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrandTotal { get; set; }

        // Sidebar data
        public string CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Customer data
        public CustomerDetailsViewModel CustomerData { get; set; }

        // Customer list for dropdown
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
    }

    public class SalesOrderItemDetails
    {
        public int SL { get; set; }
        public string Description { get; set; }
        public int Product { get; set; }
        public string UnitName { get; set; }
        public decimal Area { get; set; }
        public decimal Rate { get; set; }
        public decimal Quantity { get; set; }
        public string LIP { get; set; }
        public string LMAC { get; set; }

        public decimal Amount => Quantity * Rate;
    }

    //public class CustomerDetailsViewModel
    //{
    //    public string CompanyName { get; set; }
    //    public string ContactName { get; set; }
    //    public string Email { get; set; }
    //    public string Phone { get; set; }
    //    public string AddressLine1 { get; set; }
    //    public string AddressLine2 { get; set; }
    //    public string TaxNumber { get; set; }
    //}

    //public class CustomerDto
    //{
    //    public int Id { get; set; }
    //    public string CompanyName { get; set; }
    //    public string ContactName { get; set; }
    //    public string Email { get; set; }
    //    public string Phone { get; set; }
    //    public string AddressLine1 { get; set; }
    //    public string AddressLine2 { get; set; }
    //    public string TaxNumber { get; set; }
    //}

    public class SalesOrderSidebarDetailsViewModel
    {
        public int? SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; }
        public string QuotationNumber { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool CanEdit => true; // Add your logic here
        public bool CanSendEmail => true; // Add your logic here

        public List<PriceQuotationVersionViewModel> SalesOrderIdList { get; set; }

        public bool CanCreateShipment { get; set; }
        public bool HasShipments => Shipments != null && Shipments.Any();
        public List<ShipmentInfo> Shipments { get; set; } = new List<ShipmentInfo>();

    }
}
