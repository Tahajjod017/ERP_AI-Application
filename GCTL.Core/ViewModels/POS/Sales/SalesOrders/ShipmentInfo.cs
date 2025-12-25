namespace GCTL.Core.ViewModels.POS.Sales.SalesOrders
{
    public class ShipmentInfo
    {
        public int ShipmentId { get; set; }
        public string ShipmentNumber { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; } // "success", "warning", "info", "primary", "danger"
    }
}