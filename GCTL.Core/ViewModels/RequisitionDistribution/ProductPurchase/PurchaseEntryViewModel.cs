using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RequisitionDistribution.ProductPurchase
{
    public class PurchaseEntryViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public int SupplierID { get; set; }
        public int PurchaseBy { get; set; }
        public int ReceivedBy { get; set; }
        public int WarehouseID { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
