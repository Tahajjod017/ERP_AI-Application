using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RequisitionDistribution.ProductPurchase
{
    public class ProductPurchaseListViewModel
    {
        public int? Id { get; set; }
        public int? RequisitionID { get; set; }
        public string ProjectName { get; set; }
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public decimal? Quantity { get; set; }
        public int ProductTypeID { get; set; }
        public int? ProductId { get; set; }
    }

    public class ProductPurchaseOrderListViewModel
    {
        public int PurchasOrderID { get; set; }
        public string POID { get; set; }
        public string Supplier { get; set; }
        public DateTime? PODate { get; set; }
        public string Note { get; set; }
        public int? TotalProduct { get; set; }
        public decimal? TotalQuentity { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool Received { get; set; }
    }

    public class ProductPurchaseViewModel
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Type { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductPurchaseFormViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string SupplierName { get; set; }
        public string PurchaserName { get; set; }
        public string ReceiverName { get; set; }
        public string WarehouseName { get; set; }
    }
}
