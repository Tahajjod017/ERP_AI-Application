using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Requsition.AddRequisition
{
    public class EditRequisitionViewModel 
    {
        public int ReqId { get; set; }

        public int? OrganizationId { get; set; }
        public int OrganizationBranchId { get; set; }
        public int RequesterId { get; set; }
        public int? Priority { get; set; }
        public string? RequisitionNote { get; set; }

        // List of products (multiple)
        public List<EditRequisitionProductViewModel> Products { get; set; } = new List<EditRequisitionProductViewModel>();

        public string? Status { get; set; } // Optional: pending, partially_approved, approved
    }

    public class EditRequisitionProductViewModel
    {
        public int Index { get; set; } // For frontend binding (optional but helpful)
        public int Id { get; set; } // RequisitionItemID — crucial for updates/deletes
        public int ProductTypeId { get; set; }
        public int? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; } // Read-only
        public string? Brand { get; set; } // Read-only
    }
}
