
namespace GCTL.Core.ViewModels.CRM
{
    public class ActivityPDFModel
    {
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string? CompanyAddress { get; set; }
        public List<Activity> Activities { get; set; }

    }
    public class Activity
    {
        public string? LeadName { get; set; }
        public string? ActivityType { get; set; }
        public string? ActivityNote { get; set; }
        public DateTime? ActivityDateTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CustomerName { get; set; }
        public string? LeadStage { get; set; }
        public string? LeadPriority { get; set; }
        public string? LeadSource { get; set; }
        public decimal? LeadProbability { get; set; }
        public string? LeadOwner { get; set; }

    }
}
