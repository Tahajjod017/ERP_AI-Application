namespace GCTL.Core.ViewModels.POS.Purchasess.Purchase.PurchaseReceived
{
    public class PurchaseRecItemViewModel
    {
        public int? ProductID { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? ReqQuantity { get; set; }
        public decimal? AlrDisQuantity { get; set; }
        public decimal? UnitDistribute { get; set; }
        public decimal? TotalAmount => Quantity * UnitDistribute;

        public int PurchasOrderItemID { get; set; }
        public int RequisitionItemID { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}