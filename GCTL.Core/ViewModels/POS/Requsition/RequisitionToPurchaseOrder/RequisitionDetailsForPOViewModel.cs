using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Requsition.RequisitionToPurchaseOrder
{
    public class RequisitionDetailsForPOViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionCode { get; set; }
        public DateTime RequisitionDate { get; set; }
        public string RequisitionBy { get; set; }
        public string Organization { get; set; }
        public string Branch { get; set; }
        public string Priority { get; set; }
        public string RequisitionNote { get; set; }
        public List<RequisitionItemForPOViewModel> Items { get; set; }
        public bool HasPurchaseOrder { get; set; }
        public int? PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; }
    }

   
}
