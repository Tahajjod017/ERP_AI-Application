using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Requsition.RequisitionToPurchaseOrder
{
    public class ConvertToPurchaseOrderViewModel : BaseViewModel
    {
        [Required]
        public int RequisitionId { get; set; }

        [Required(ErrorMessage = "Please select a supplier")]
        public int SupplierId { get; set; }

        public int? BillingAddressId { get; set; }

        public int? ShippingAddressId { get; set; }

        public int? OrganizationId { get; set; }

        public int? OrganizationBranchId { get; set; }

        [Required]
        public string POCode { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public string OtherReference { get; set; }

        public string WorkorderNo { get; set; }

        public DateTime? WorkOrderDate { get; set; }

        [Range(0, 100)]
        public decimal TaxPercent { get; set; }

        public string Note { get; set; }

        public string TermsAndConditions { get; set; }

        public string IsDraft { get; set; }

        public int? StatusId { get; set; }

        [Required]
        public List<POItemViewModel> Items { get; set; }
    }
}
