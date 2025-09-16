using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadDetailsDTO
    {
        public int? LeadActivityID { get; set; }
        public string? LeadActivityType { get; set; }
        public string? ActivityNote { get; set; }
        public DateTime? ActivityDateTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? LeadName { get; set; }
        public int? LeadID { get; set; }
        public string? CustomerName { get; set; }
        public string? LeadStage { get; set; }
        public string? LeadPriority { get; set; }
        public string? LeadSource { get; set; }
        public decimal? LeadProbability { get; set; }
        public string? LeadOwner { get; set; }
    }
}
