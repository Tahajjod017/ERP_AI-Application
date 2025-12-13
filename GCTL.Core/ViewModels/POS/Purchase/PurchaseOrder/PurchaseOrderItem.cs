namespace GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder
{
    public class PurchaseOrderItem
    {
        public int SL { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; } = string.Empty;
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal Amount => (Quantity ?? 0) * (UnitPrice ?? 0);
    }
}