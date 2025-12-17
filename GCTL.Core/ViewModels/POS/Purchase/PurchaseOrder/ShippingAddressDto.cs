namespace GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder
{
    public class ShippingAddressDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string FullAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}