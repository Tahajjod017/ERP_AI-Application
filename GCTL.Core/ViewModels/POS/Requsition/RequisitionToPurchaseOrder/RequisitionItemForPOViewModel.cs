namespace GCTL.Core.ViewModels.POS.Requsition.RequisitionToPurchaseOrder
{
    public class RequisitionItemForPOViewModel
    {
        public int ItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public string Brand { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal ApprovedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}