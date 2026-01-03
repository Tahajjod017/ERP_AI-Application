using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Sales.InvoiceF
{

    public class InvoiceViewModel : BaseViewModel
    {
        public int? Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string InvoiceNumber { get; set; }
        public int? SelectedCustomerId { get; set; }
        public int? SelectedSalesOrderId { get; set; }

        // Flags
        public bool IsDraft { get; set; }
        public bool IsVatAfterSubtotal { get; set; }
        public bool IsItemPriceIncludingVat { get; set; }
        public bool IsPriceWithoutVat { get; set; }
        public bool ShowTaxColumn { get; set; }
        public bool IsAit { get; set; }

        public bool IsDirectChallan { get; set; }

        // Percentages
        public decimal VatPercent { get; set; } = 15;   // default VAT %
        public decimal AitPercent { get; set; } = 5;    // default AIT %

        // References
        public string? OtherReference { get; set; }
        public string? InvoiceNote { get; set; }

        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        // Addresses
        public AddressViewModel BillingAddress { get; set; } = new AddressViewModel();
        public AddressViewModel ShippingAddress { get; set; } = new AddressViewModel();

        // Customers
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();

        // ============================================
        // CALCULATIONS - OPTION A LOGIC
        // ============================================

        /// <summary>
        /// Net Subtotal (excluding VAT)
        /// In "Including VAT" mode, this extracts the net amount.
        /// In other modes, prices are already net.
        /// </summary>
        //public decimal SubTotal
        //{
        //    get
        //    {
        //        if (Items == null || !Items.Any())
        //            return 0;

        //        // MODE 1: Each item price including VAT
        //        if (IsItemPriceIncludingVat)
        //        {
        //            // Extract net price from gross price
        //            return Items.Sum(i =>
        //            {
        //                var netPrice = i.UnitPrice / (1 + VatPercent / 100);
        //                return netPrice * i.Quantity;
        //            });
        //        }
        //        else
        //        {
        //            // MODE 2 & 3: Prices are already net
        //            return Items.Sum(i => i.UnitPrice * i.Quantity);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Total VAT Amount based on selected mode
        ///// </summary>
        //public decimal VatAmount
        //{
        //    get
        //    {
        //        if (Items == null || !Items.Any())
        //            return 0;

        //        // MODE 1: Each item price including VAT
        //        if (IsItemPriceIncludingVat)
        //        {
        //            // Extract VAT from each item's gross price
        //            return Items.Sum(i =>
        //            {
        //                var netPrice = i.UnitPrice / (1 + VatPercent / 100);
        //                var vatPerItem = i.UnitPrice - netPrice;
        //                return vatPerItem * i.Quantity;
        //            });
        //        }
        //        // MODE 2: Price without VAT (per-item calculation)
        //        else if (IsPriceWithoutVat)
        //        {
        //            // Calculate VAT per item on net price
        //            return Items.Sum(i =>
        //                (i.UnitPrice * i.Quantity * VatPercent) / 100
        //            );
        //        }
        //        // MODE 3: VAT after subtotal (invoice-level calculation)
        //        else if (IsVatAfterSubtotal)
        //        {
        //            // Calculate VAT once on entire subtotal
        //            return SubTotal * VatPercent / 100;
        //        }

        //        return 0;
        //    }
        //}

        /// <summary>
        /// Net Subtotal (excluding VAT) - respects per-item VAT rates
        /// </summary>
        public decimal SubTotal
        {
            get
            {
                if (Items == null || !Items.Any()) return 0;

                if (IsItemPriceIncludingVat)
                {
                    // Price includes VAT → extract net using each item's own VAT rate
                    return Items.Sum(i =>
                    {
                        decimal itemVatRate = i.VatPercent; // Per-item VAT %
                        if (itemVatRate == 0) itemVatRate = VatPercent; // fallback to global
                        var netPrice = i.UnitPrice / (1 + itemVatRate / 100);
                        return netPrice * i.Quantity;
                    });
                }
                else
                {
                    // In other modes, UnitPrice is net
                    return Items.Sum(i => i.UnitPrice * i.Quantity);
                }
            }
        }

        /// <summary>
        /// Total VAT Amount - now uses per-item VAT rates where applicable
        /// </summary>
        public decimal VatAmount
        {
            get
            {
                if (Items == null || !Items.Any()) return 0;

                if (IsItemPriceIncludingVat)
                {
                    // Extract VAT from gross price using per-item rate
                    return Items.Sum(i =>
                    {
                        decimal itemVatRate = i.VatPercent;
                        if (itemVatRate == 0) itemVatRate = VatPercent;
                        var netPrice = i.UnitPrice / (1 + itemVatRate / 100);
                        var vatPerItem = i.UnitPrice - netPrice;
                        return vatPerItem * i.Quantity;
                    });
                }
                else if (IsPriceWithoutVat)
                {
                    // Add VAT on net price using per-item rate
                    return Items.Sum(i =>
                    {
                        decimal itemVatRate = i.VatPercent;
                        if (itemVatRate == 0) itemVatRate = VatPercent;
                        return (i.UnitPrice * i.Quantity * itemVatRate) / 100;
                    });
                }
                else if (IsVatAfterSubtotal)
                {
                    // Only here we use global VAT % on total
                    return SubTotal * VatPercent / 100;
                }

                return 0;
            }
        }

        /// <summary>
        /// Get VAT amount for display in table row (per item)
        /// </summary>
        public decimal GetItemVatAmount(InvoiceItem item)
        {
            if (item == null) return 0;

            decimal itemVatRate = item.VatPercent > 0 ? item.VatPercent : VatPercent;

            if (IsItemPriceIncludingVat)
            {
                var netPrice = item.UnitPrice / (1 + itemVatRate / 100);
                return (item.UnitPrice - netPrice) * item.Quantity;
            }
            else if (IsPriceWithoutVat)
            {
                return (item.UnitPrice * item.Quantity * itemVatRate) / 100;
            }
            else
            {
                return 0; // VAT shown at bottom only
            }
        }

        /// <summary>
        /// Get total line amount (including VAT if applicable)
        /// </summary>
        public decimal GetItemTotalAmount(InvoiceItem item)
        {
            if (item == null) return 0;

            decimal itemVatRate = item.VatPercent > 0 ? item.VatPercent : VatPercent;

            if (IsItemPriceIncludingVat)
            {
                return item.UnitPrice * item.Quantity; // already gross
            }
            else if (IsPriceWithoutVat)
            {
                var net = item.UnitPrice * item.Quantity;
                var vat = net * itemVatRate / 100;
                return net + vat;
            }
            else
            {
                return item.UnitPrice * item.Quantity; // VAT added later
            }
        }



        /// <summary>
        /// Gross Subtotal (including VAT)
        /// </summary>
        public decimal GrossSubtotal => SubTotal + VatAmount;

        /// <summary>
        /// AIT Amount (only if IsAit is enabled)
        /// Calculated on gross subtotal (after VAT)
        /// </summary>
        public decimal AitAmount => IsAit ? (GrossSubtotal * AitPercent / 100) : 0;

        /// <summary>
        /// Grand Total = Gross Subtotal + AIT
        /// </summary>
        public decimal GrandTotal => GrossSubtotal + AitAmount;

        /// <summary>
        /// Get VAT amount for a specific item (for display in table)
        /// </summary>
        //public decimal GetItemVatAmount(InvoiceItem item)
        //{
        //    if (item == null)
        //        return 0;

        //    // MODE 1: Each item price including VAT
        //    if (IsItemPriceIncludingVat)
        //    {
        //        var netPrice = item.UnitPrice / (1 + VatPercent / 100);
        //        var vatPerItem = item.UnitPrice - netPrice;
        //        return vatPerItem * item.Quantity;
        //    }
        //    // MODE 2: Price without VAT (per-item)
        //    else if (IsPriceWithoutVat)
        //    {
        //        return (item.UnitPrice * item.Quantity * VatPercent) / 100;
        //    }
        //    // MODE 3: VAT after subtotal (no per-item VAT)
        //    else
        //    {
        //        return 0;
        //    }
        //}

        ///// <summary>
        ///// Get total amount for a specific item (net + VAT if applicable)
        ///// </summary>
        //public decimal GetItemTotalAmount(InvoiceItem item)
        //{
        //    if (item == null)
        //        return 0;

        //    // MODE 1: Each item price including VAT
        //    if (IsItemPriceIncludingVat)
        //    {
        //        // Item total is the gross price × quantity
        //        return item.UnitPrice * item.Quantity;
        //    }
        //    // MODE 2: Price without VAT
        //    else if (IsPriceWithoutVat)
        //    {
        //        var netAmount = item.UnitPrice * item.Quantity;
        //        var vatAmount = (netAmount * VatPercent) / 100;
        //        return netAmount + vatAmount;
        //    }
        //    // MODE 3: VAT after subtotal
        //    else
        //    {
        //        // Item total is just net amount (VAT added at invoice level)
        //        return item.UnitPrice * item.Quantity;
        //    }
        //}

        /// <summary>
        /// Validates that only one VAT mode is active
        /// </summary>
        public bool IsVatModeValid()
        {
            int activeModesCount =
                (IsItemPriceIncludingVat ? 1 : 0) +
                (IsPriceWithoutVat ? 1 : 0) +
                (IsVatAfterSubtotal ? 1 : 0);

            return activeModesCount <= 1;
        }

        /// <summary>
        /// Returns the name of the active VAT mode
        /// </summary>
        public string GetActiveVatMode()
        {
            if (IsItemPriceIncludingVat)
                return "Each item price including VAT";
            if (IsPriceWithoutVat)
                return "Price without VAT (per-item)";
            if (IsVatAfterSubtotal)
                return "VAT after subtotal";
            return "No VAT";
        }
    }



    public class InvoiceItem
    {
        public int? Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VatPercent { get; set; } = 5m;
        /// <summary>
        /// Base amount (UnitPrice × Quantity)
        /// This is the net amount in most cases
        /// </summary>
        public decimal Amount => UnitPrice * Quantity;

        public string ItemSerial { get; set; }  // New property for Direct Challan


        public decimal EffectiveVatPercent(decimal globalVatPercent)
        {
            return VatPercent > 0 ? VatPercent : globalVatPercent;
        }

        public int SL { get; set; }
    }



    //public class InvoiceViewModel : BaseViewModel
    //{
    //    public int? Id { get; set; }
    //    public DateTime InvoiceDate { get; set; }
    //    public string InvoiceNumber { get; set; }
    //    public int? SelectedCustomerId { get; set; }
    //    public int? SelectedSalesOrderId { get; set; }



    //    public bool IsDraft { get; set; }
    //    public bool IsVatAfterSubtotal { get; set; }
    //    public bool IsItemPriceIncludingVat { get; set; }
    //    public bool IsPriceWithoutVat { get; set; }
    //    public bool AddAIT5Percent { get; set; }
    //    public bool ShowTaxColumn { get; set; }


    //    public decimal VatPercent { get; set; }
    //    public string? OtherReference { get; set; }
    //    public string? InvoiceNote { get; set; }
    //    public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    //    // Addresses
    //    public AddressViewModel BillingAddress { get; set; }
    //    public AddressViewModel ShippingAddress { get; set; }

    //    // For display
    //    public decimal SubTotal => Items?.Sum(i => i.Amount) ?? 0;
    //    public decimal VatAmount => SubTotal * VatPercent / 100;
    //    public decimal GrandTotal => SubTotal + VatAmount;

    //    // Customer list for dropdown
    //    public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
    //}

    //public class InvoiceItem
    //{
    //    public int SL { get; set; }
    //    public int ProductId { get; set; }
    //    public decimal? Quantity { get; set; }
    //    public decimal? UnitPrice { get; set; }

    //    public decimal Amount => (Quantity ?? 0) * (UnitPrice ?? 0);
    //}

    public class AddressViewModel
    {
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullAddress { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string TaxNumber { get; set; }
    }
}
