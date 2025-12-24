using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchase.PurchaseReceive
{
    public class PurchaseReceiveListViewModel
    {
        public int PurchaseReceiveID { get; set; }
        public string PRNumber { get; set; }
        public DateTime? PRDate { get; set; }
        public string PONumber { get; set; }
        public string SupplierName { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalReceivedQty { get; set; }
        public string Status { get; set; }
        public string VendorBillChalan { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    // Open PO list for dropdown
    public class OpenPurchaseOrderViewModel
    {
        public int PurchaseOrderID { get; set; }
        public int PurchaseOrderVersionID { get; set; }
        public string PONumber { get; set; }
        public string SupplierName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int TotalItems { get; set; }
    }

    // PO Details for receive
    public class PODetailsForReceiveViewModel
    {
        public int PurchaseOrderID { get; set; }
        public int PurchaseOrderVersionID { get; set; }
        public string PONumber { get; set; }
        public string SupplierName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Note { get; set; }
        public List<POItemForReceiveViewModel> Items { get; set; }
    }

    public class POItemForReceiveViewModel
    {
        public int POItemVersionID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string Brand { get; set; }
        public string Unit { get; set; }
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsFullyReceived { get; set; }
    }

    // Receive details view model
    public class ReceiveDetailsViewModel
    {
        public int PurchaseReceiveID { get; set; }
        public string PRNumber { get; set; }
        public DateTime? PRDate { get; set; }
        public string PONumber { get; set; }
        public string SupplierName { get; set; }
        public string VendorBillChalan { get; set; }
        public DateTime? BillDate { get; set; }
        public string PRNote { get; set; }
        public string ReceivedByName { get; set; }
        public string Status { get; set; }
        public decimal TotalReceivedQty { get; set; }
        public decimal TotalAcceptedQty { get; set; }
        public decimal TotalRejectedQty { get; set; }
        public bool IsPartialReceive { get; set; }
        public List<ReceiveItemDetailViewModel> Items { get; set; }
    }

    public class ReceiveItemDetailViewModel
    {
        public int ItemID { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string Brand { get; set; }
        public string Unit { get; set; }
        public decimal POQuantity { get; set; }
        public decimal ReceiveQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public string RejectionReason { get; set; }
        public string Note { get; set; }
    }

    // Create purchase receive model
    public class CreatePurchaseReceiveViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "Please select a Purchase Order")]
        public int PurchaseOrderVersionID { get; set; }

        [Required]
        public string PRNumber { get; set; }

        [Required]
        public DateTime PRDate { get; set; }

        public string VendorBillChalan { get; set; }

        public DateTime? BillDate { get; set; }

        public string PRNote { get; set; }

        public int? ReceivedByEmployeeID { get; set; }

        public int? StatusID { get; set; }

        public string? AttachmentPath { get; set; }

        [Required]
        public List<ReceiveItemViewModel> Items { get; set; }
    }

    public class ReceiveItemViewModel
    {
        [Required]
        public int POItemVersionID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Receive quantity must be greater than 0")]
        public decimal ReceiveQuantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Accepted quantity cannot be negative")]
        public decimal AcceptedQuantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Rejected quantity cannot be negative")]
        public decimal RejectedQuantity { get; set; }

        public string? RejectionReason { get; set; }

        public string? Note { get; set; }

        public decimal POQuantity { get; set; }
    }

    // Edit purchase receive model
    public class EditPurchaseReceiveViewModel : BaseViewModel
    {
        [Required]
        public int PurchaseReceiveID { get; set; }

        [Required]
        public int PurchaseOrderVersionID { get; set; }

        [Required]
        public string PRNumber { get; set; }

        [Required]
        public DateTime PRDate { get; set; }

        public string VendorBillChalan { get; set; }

        public DateTime? BillDate { get; set; }

        public string PRNote { get; set; }

        public int? ReceivedByEmployeeID { get; set; }

        public int? StatusID { get; set; }

        [Required]
        public List<ReceiveItemViewModel> Items { get; set; }
    }
}
