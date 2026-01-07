using System;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadActivityCommentVM : BaseViewModel
    {
        public int LeadActivityCommentID { get; set; }
        public int LeadDetailID { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedByName { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}