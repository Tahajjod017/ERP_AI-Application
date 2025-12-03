

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
        public string? File { get; set; }
        public int? CustomerAddreeTypeId { get; set; }
    }
}
