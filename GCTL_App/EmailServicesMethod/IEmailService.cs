using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL_App.EmailServicesMethod
{
    public interface IEmailService
    {
            Task<string> SendEmailAsync(
                string toEmail,
                string subject,
                string razorTemplateFile,
                object model,
                byte[]? attachment = null,
                string? attachmentFileName = null);
  

    }
}
