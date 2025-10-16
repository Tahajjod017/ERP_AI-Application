using GCTL.Core.ViewModels;
using GCTL.Service.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailVM model, int? orgId);
        Task SendAsync(
            int organizationId,
            string toEmail,
            string subject,
            string body,
            byte[]? attachmentBytes = null,
            string? attachmentName = null,
            List<LinkedResource>? linkedResources = null);
        Task SendEmailLeaveRequest(EmailVM model, int? orgId);
    }
}
