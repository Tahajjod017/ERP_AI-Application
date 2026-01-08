using System;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadActivityCommentVM : BaseViewModel
    {
        public int LeadActivityCommentID { get; set; }
        public int LeadDetailID { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string CreatedByName { get; set; }
    }
}