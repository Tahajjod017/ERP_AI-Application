namespace GCTL.Core.ViewModels.CRM
{
    public class LeadActivityVM
    {
        public int LeadDetailID { get; set; }
        public DateTime? ActivityDateTime { get; set; }
        public string ActivityNote { get; set; }
        public string? PhoneNumberList { get; set; }
        public string? Emails { get; set; }
        public string FileLink { get; set; }
        public List<CommonSelectVM2>? PhoneNumbers { get; set; }
        public List<CommonSelectVM2>? EmailAddresses { get; set; }
        public bool? IsDone { get; set; }
        public string LeadActivityName { get; set; }
        public string LeadActivityIcon { get; set; }
        public string CreatedByName { get; set; }

        // Add Status Information
        public int? StatusID { get; set; }
        public string StatusName { get; set; }
    }
}