using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Enums;

namespace GCTL.Core.ViewModels.POS.Sales.ShipmentDetails
{
    public class ChallanDetailsViewModel
    {
        public int Id { get; set; }
        public string? ShipmentNumber { get; set; }
        public DateTime? ShipmentDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        public int? SalesOrderId { get; set; }
        public int? InvoiceId { get; set; }
        public string? SourceType { get; set; }
        public string? SourceNumber { get; set; }

        public int? ShippingMethodId { get; set; }
        public string? ShippingMethodName { get; set; }
        public string? TrackingNumber { get; set; }
        public int? ShippingAddressId { get; set; }
        public decimal? ShippingCost { get; set; }

        public List<ShipmentItemDetails> Items { get; set; } = new();
        public AddressDetailsViewModel ShippingAddress { get; set; } = new();

        public string? Note { get; set; }

        // Sidebar data
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ShipmentItemDetails
    {
        public int SL { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal OrderedQuantity { get; set; }
        public decimal ShippedQuantity { get; set; }
        public int? FromLocationId { get; set; }
        public string? FromLocationName { get; set; }
        public string? Note { get; set; }
    }

    public class AddressDetailsViewModel
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? FullAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    // Sidebar specific view model
    public class ChallanSidebarDetailsViewModel
    {
        public int ShipmentId { get; set; }
        public string ShipmentNumber { get; set; }
        public ShipmentStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Permissions/Actions
        public bool CanEdit => Status == ShipmentStatus.Pending || Status == ShipmentStatus.Packed;
        public bool CanPack => Status == ShipmentStatus.Pending;
        public bool CanShip => Status == ShipmentStatus.Packed;
        public bool CanDeliver => Status == ShipmentStatus.Shipped || Status == ShipmentStatus.InTransit;
        public bool CanCancel => Status != ShipmentStatus.Delivered && Status != ShipmentStatus.Cancelled;
        public bool CanPrint => true;

        // Source document
        public string SourceType { get; set; }
        public int? SourceId { get; set; }
        public string SourceNumber { get; set; }
        public string TrackingNumber { get; set; }
    }
}
