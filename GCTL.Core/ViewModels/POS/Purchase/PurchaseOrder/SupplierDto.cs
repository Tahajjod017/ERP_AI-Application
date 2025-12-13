namespace GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
    }

}