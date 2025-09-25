using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement
{
    public class EmailService : AppService<EmailSettings>, IEmailService
    {
        private readonly IGenericRepository<EmailSettings> _emailSettings;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfo;
   

        public EmailService(IGenericRepository<EmailSettings> genericRepository, IGenericRepository<EmailSettings> emailSettings, IConfiguration configuration, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfo) : base(genericRepository)
        {
            _emailSettings = emailSettings;
            _configuration = configuration;
            _employeeOfficeInfo = employeeOfficeInfo;
        }


        public async Task SendEmailAsync(EmailVM model, int? empId)
        {
            var employee = await _employeeOfficeInfo.AllActive().FirstOrDefaultAsync(x => x.EmployeeID == empId);
            if(employee == null)
            {
                return;
            }

            // get first (or only) settings record
            var emailConfig = await _emailSettings.AllActive().FirstOrDefaultAsync(x => x.OrganizationID == employee.OrganizationID);
            if (emailConfig == null)
            {
                throw new InvalidOperationException("Email settings not configured in database.");
            }

            using (var smtpClient = new SmtpClient(emailConfig.Host, emailConfig.Port))
            {
                smtpClient.EnableSsl = emailConfig.IsSSLRequired;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = emailConfig.IsDefaultCredential;

                if (!emailConfig.IsDefaultCredential)
                {
                    smtpClient.Credentials = new NetworkCredential(emailConfig.UserName, emailConfig.Password);
                }

                if (string.IsNullOrEmpty(model.To))
                {
                    throw new ArgumentNullException(nameof(model.To), "Recipient email address is required.");
                }

                var mailMessage = new MailMessage(emailConfig.UserName, model.To)
                {
                    Subject = $"Application for {model.Subject}." ?? "Leave.",
                    Body = string.IsNullOrEmpty(model.Body) ? BuildEmailBody(model) : model.Body,
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        #region  leave Request
        #region  leave Request
        public async Task SendEmailLeaveRequest(EmailVM model, int? empId)
        {
            var employee = await _employeeOfficeInfo.AllActive()
                .FirstOrDefaultAsync(x => x.EmployeeID == empId);
            if (employee == null) return;

            var emailConfig = await _emailSettings.AllActive()
                .FirstOrDefaultAsync(x => x.OrganizationID == employee.OrganizationID);
            if (emailConfig == null)
                throw new InvalidOperationException("Email settings not configured in database.");

            using (var smtpClient = new SmtpClient(emailConfig.Host, emailConfig.Port))
            {
                smtpClient.EnableSsl = emailConfig.IsSSLRequired;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = emailConfig.IsDefaultCredential;

                if (!emailConfig.IsDefaultCredential)
                {
                    smtpClient.Credentials = new NetworkCredential(emailConfig.UserName, emailConfig.Password);
                }

                if (string.IsNullOrEmpty(model.To))
                    throw new ArgumentNullException(nameof(model.To), "Recipient email address is required.");

                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(emailConfig.UserName, "HR System"), // ✅ Safe From
                        Subject = model.Subject ?? "Leave Application",
                        Body = !string.IsNullOrEmpty(model.Body)
                                ? model.Body
                                : BuildEmailBodyLeaveRequest(model), // fallback only if needed
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(model.To);

                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Email send failed: {ex.Message}", ex);
                }
            }
        }
        #endregion

        #endregion


        #region LeaveRequest Body 
        private string BuildEmailBodyLeaveRequest(EmailVM model)
        {
            return $@"
                


            ";
        }

        #endregion


        #region BuildEmailBody
        private string BuildEmailBody(EmailVM model)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Leave Application</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            margin: 40px;
                            line-height: 1.8;
                            background-color: #f9f9f9;
                        }}
                        .container {{
                            background: #fff;
                            padding: 30px;
                            border-radius: 10px;
                            box-shadow: 0 0 10px rgba(0,0,0,0.1);
                        }}
                        .header {{
                            text-align: center;
                            margin-bottom: 20px;
                        }}
                        .footer {{
                            margin-top: 30px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Leave Application</h2>
                        </div>
                        <p>Date: {DateTime.Now:dd MMMM yyyy}</p>
                        <p>Dear Sir/Madam,</p>
                        <p>
                            I would like to request leave regarding <strong>{model.Subject}</strong>. 
                            {(!string.IsNullOrEmpty(model.Body) ? model.Body : "Due to {reason}, I kindly request a few days of leave.")}
                        </p>
                        <p>
                            I would be grateful if you could kindly approve my leave request for the mentioned period.
                        </p>
                        <div class='footer'>
                            <p>Sincerely,</p>
                            <p>..................................</p>
                            <p>EmployeeName</p>
                            <p>Designation</p>
                            <p>Phone</p>
                            <p>Email</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }
        #endregion
    }
}
