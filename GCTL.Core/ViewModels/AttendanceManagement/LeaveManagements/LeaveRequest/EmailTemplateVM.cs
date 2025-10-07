using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class EmailTemplateVM
    {
        public string? LogoUrl { get; set; }
        public string? FormattedAddress { get; set; }
        public string? CountryName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? RecipientName { get; set; }
        public string? StatusMessage { get; set; }

        public string?   ApplicantName { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? LeaveName { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? Reason { get; set; }

        public string? AcceptUrl { get; set; }
        public string? DenyUrl { get; set; }
        public string? ModifyLink { get; set; }
        public bool IsApplicant { get; set; }
    }
}
