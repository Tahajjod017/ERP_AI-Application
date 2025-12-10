namespace GCTL.Core.ViewModels.RequisitionDistribution.Purchase.CreatePO
{
    public class PurchaseItemViewModel
    {
        public int? ProductID { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalAmount => Quantity * UnitPrice;

        public int PurchasOrderItemID { get; set; }

       
    }

    


    public class VendorViewModel : BaseViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string TaxNumber { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
    }

    public class ShippingAddressViewModel
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }

    


}