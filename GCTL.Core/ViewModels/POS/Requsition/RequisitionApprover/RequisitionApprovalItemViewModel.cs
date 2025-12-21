using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Requsition.RequisitionApprover
{
    public class RequisitionApprovalItemViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionCode { get; set; }
        public DateTime RequisitionDate { get; set; }
        public string RequisitionBy { get; set; }
        public int TotalItems { get; set; }
        public string Priority { get; set; }
        public int CurrentStep { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool CanEdit { get; set; }
    }

    // Details view model for modal
    public class RequisitionDetailsViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionCode { get; set; }
        public DateTime RequisitionDate { get; set; }
        public string RequisitionBy { get; set; }
        public string Organization { get; set; }
        public string Branch { get; set; }
        public string Priority { get; set; }
        public string RequisitionNote { get; set; }
        public int CurrentStep { get; set; }
        public int ApproverStep { get; set; }
        public bool IsFirstApprover { get; set; }
        public bool CanApprove { get; set; }
        public bool CanEdit { get; set; }
        public List<RequisitionItemDetailViewModel> Items { get; set; }
        public List<ApprovalHistoryViewModel> ApprovalHistory { get; set; }
    }

    public class RequisitionItemDetailViewModel
    {
        public int ItemId { get; set; }
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public string Brand { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal? ApprovedQuantity { get; set; }
        public bool IsApproved { get; set; }
    }

    public class ApprovalHistoryViewModel
    {
        public int Step { get; set; }
        public string ApproverName { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Note { get; set; }
    }

    // Approve requisition model
    public class ApproveRequisitionViewModel : BaseViewModel
    {
        [Required]
        public int RequisitionId { get; set; }

        public string? ApproverNote { get; set; }

        public List<ApproveItemViewModel> Items { get; set; }
    }

    public class ApproveItemViewModel
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Approved quantity must be greater than 0")]
        public decimal ApprovedQuantity { get; set; }
    }

    // Decline requisition model
    public class DeclineRequisitionViewModel : BaseViewModel
    {
        [Required]
        public int RequisitionId { get; set; }

        [Required(ErrorMessage = "Please provide a reason for declining")]
        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string DeclineNote { get; set; }
    }

    // Edit approved requisition model
    public class EditApprovedRequisitionViewModel : BaseViewModel
    {
        [Required]
        public int RequisitionId { get; set; }

        public string? ApproverNote { get; set; }

        public List<ApproveItemViewModel> Items { get; set; }
    }
}
