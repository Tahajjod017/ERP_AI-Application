
namespace GCTL.Core.ViewModels.CRM
{
    public class ActivityPDFModel
    {
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string? CompanyAddress { get; set; }
        public string? TeamName { get; set; }
        public string? EmployeeName { get; set; }
        public string? Email { get; set; }
        public bool? IsTeamHead { get; set; }

        // New property: for grouping multiple members in one PDF
        public List<ActivityPDFModel>? SubEmployees { get; set; }

        public List<Activity> Activities { get; set; } = new List<Activity>();

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
        public int? LeadOwnerId { get; set; }

    }
}
