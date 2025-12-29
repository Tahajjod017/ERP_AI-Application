using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationDetails;

namespace GCTL.Core.ViewModels.POS.Sales.Shipment
{
    public class ChallanViewModel : BaseViewModel
    {
        // ----- Header -------------------------------------------------
        public int? Id { get; set; }
        public string? ShipmentNumber { get; set; } = string.Empty;
        public DateTime? ShipmentDate { get; set; } = DateTime.Today;
        public DateTime? ExpectedDeliveryDate { get; set; } = DateTime.Today.AddDays(7);
        public DateTime? ActualDeliveryDate { get; set; }

        // ----- Source Reference ----------------------------------------
        public int? SalesOrderId { get; set; }
        public int? InvoiceId { get; set; }
        public string? SourceType { get; set; } // "SalesOrder" or "Invoice"
        public string? SourceNumber { get; set; }

        // ----- Shipping Details ----------------------------------------
        public int? ShippingMethodId { get; set; }
        public string? TrackingNumber { get; set; } = string.Empty;
        public int? ShippingAddressId { get; set; }
        public decimal? ShippingCost { get; set; } = 0m;

        // ----- Items ---------------------------------------------------
        public List<ShipmentItem> Items { get; set; } = new() { new ShipmentItem() };

        // ----- Additional ----------------------------------------------
        public string? Note { get; set; } = string.Empty;
        public int? StatusId { get; set; }
        public bool IsDraft { get; set; }

        // ----- Dropdown Data -------------------------------------------
        public List<ShippingMethodDto> ShippingMethods { get; set; } = new();
        public List<AddressDto> Addresses { get; set; } = new();

        public CustomerDetailsViewModel? CustomerData { get; set; }

        public int? SelectedDeliveryId { get; set; }
        public int SourceId { get; set; }
    }

    public class ShipmentItem
    {
        public int SL { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; } = string.Empty;
        public decimal? OrderedQuantity { get; set; }
        public decimal? ShippedQuantity { get; set; }
        public decimal? AlreadyShipped { get; set; }
        public int? FromLocationId { get; set; }
        public string? FromLocationName { get; set; } = string.Empty;
        public string? Note { get; set; } = string.Empty;
        //public decimal RemainingQuantity { get; set; }
    }

    public class ShippingMethodDto
    {
        public int Id { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? BaseCost { get; set; }
    }

    public class AddressDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
