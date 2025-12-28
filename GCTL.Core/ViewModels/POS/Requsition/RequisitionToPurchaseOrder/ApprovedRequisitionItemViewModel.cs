using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Requsition.RequisitionToPurchaseOrder
{
    public class ApprovedRequisitionItemViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionCode { get; set; }
        public DateTime RequisitionDate { get; set; }
        public string RequisitionBy { get; set; }
        public int TotalItems { get; set; }
        public string Priority { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool HasPurchaseOrder { get; set; }
        public int? PurchaseOrderId { get; set; }
    }
}
