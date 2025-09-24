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
        public async Task SendEmailLeaveRequest(EmailVM model, int? empId)
        {
            var employee = await _employeeOfficeInfo.AllActive().FirstOrDefaultAsync(x => x.EmployeeID == empId);
            if (employee == null)
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
                    Body = string.IsNullOrEmpty(model.Body) ? BuildEmailBodyLeaveRequest(model) : model.Body,
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        #endregion


        #region LeaveRequest Body 
        private string BuildEmailBodyLeaveRequest(EmailVM model)
        {
            return $@"
                <!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>HR Leave Request</title>
    <style>
        /* Reset styles */
        body, table, td, p, a, li, h1, h2 {{
            -webkit-text-size-adjust: 100%;
            -ms-text-size-adjust: 100%;
            margin: 0;
            padding: 0;
        }}
        body {{
            font-family: Arial, sans-serif;
            font-size: 14px;
            line-height: 20px;
            color: #333333;
            background-color: #f4f4f4;
            padding: 20px;
        }}
        table {{
            border-collapse: collapse;
        }}

        /* Main container */
        .email-container {{
            max-width: 600px;
            margin: auto;
            background-color: #ffffff;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            overflow: hidden;
        }}

        /* Header */
        .header-bg {{
            position: relative;
            background-color: #3252ff;
            background-image: linear-gradient(to bottom right, #080301 120px, transparent 0);
            background-repeat: no-repeat;
            background-size: 140px 140px;
            padding: 25px 30px;
            color: #ffffff;
        }}
        .header-bg img {{
            display: block;
            border: 0;
            outline: none;
            text-decoration: none;
            max-width: 200px;
            height: auto;
        }}
        .header-bg td {{
            font-size: 13px;
            line-height: 18px;
            text-align: right;
            color: #ffffff;
        }}

        /* Content */
        .content {{
            padding: 20px 30px;
        }}
        .content p {{
            margin-bottom: 10px;
        }}
        .content h2 {{
            font-size: 18px;
            margin-bottom: 10px;
            color: #3252ff;
        }}

        /* Tables for info */
        .info-table {{
            width: 100%;
            border: 1px solid #e0e0e0;
            border-radius: 5px;
          
        }}
        .info-table th, .info-table td {{
            padding: 10px;
            border: 1px solid #e0e0e0;
            text-align: left;
		
        }}
        .info-table th {{
           background-color: #f4f4f4; 
            font-weight: bold;
			width:50%;
        }}

        /* Approval timeline */
        .timeline {{
            width: 100%;
            margin-top: 20px;
        }}
        .timeline td {{
            vertical-align: top;
        }}
        .timeline-dot {{
            width: 15px;
            height: 15px;
            border-radius: 50%;
            margin-top: 3px;
        }}
        .timeline-line {{
            width: 2px;
            height: 30px;
            background-color: #e0e0e0;
            margin-left: 6px;
        }}
		/* Section backgrounds */
.section-header {{
    background-color: #3252ff; /* Blue header */
    color: #ffffff;
}}
.section-greeting {{
    background-color: #f9f9f9; /* light grey */
}}
.section-timeline {{
    background-color: #eef4ff; /* soft blue */
}}
.section-info {{
    background-color: #ffffff; /* white card */
}}
.section-footer {{
    background-color: #000; /* footer grey */
}}
.section-button {{
    padding-top: 0;
}}

        /* Footer */
        .footer {{
            text-align: center;
            padding: 20px 30px;
            font-size: 13px;
            color: #fff;
        }}
        /* Responsive */
        @media only screen and (max-width: 600px) {{
            .header-bg td {{
                display: block;
                text-align: center;
                margin-bottom: 10px;
            }}
            .header-bg img {{
                margin: auto;
            }}
        }}
    </style>
</head>
<body>
    <table class=""email-container"">
        <!-- Header -->
        <tr>
            <td class=""header-bg"">
                <table width=""100%"">
                    <tr>
                        <td align=""left"">
                            <img src=""https://gctlsecurity.com/pub/static/frontend/CLS/Security/en_US/images/logo.png"" alt=""Company Logo"">
                        </td>
                        <td align=""right"">
                            House-42(5th Floor) Road-10,<br>
                            Sector-4, Uttara, Dhaka-1230,<br>
                            Bangladesh<br>
                            info@gctlinfosys.com<br>
                            +88 01795-788488
                        </td>
                    </tr>
                </table>
            </td>
        </tr>

        <!-- Greeting -->
        <tr>
            <td class=""content section-greeting"">
                <p>Dear HR Team,</p>
                <p>This is an automated leave request submitted by an employee. Please find the details below:</p>
            </td>
        </tr>
		<!-- Approval Timeline (Horizontal) -->
<tr>
  <td class=""content section-timeline"">
    <h2>Approval Status Timeline</h2>
    <table width=""100%"" style=""text-align:center; margin-top:20px;"">
      <tr>
        <!-- Step 1 -->
        <td style=""width:33%; position:relative;"">
          <div style=""width:20px;height:20px;background:#008000;border-radius:50%;margin:auto;""></div>
          <p style=""margin:5px 0 0;font-weight:bold;color:#008000;"">Leave Submitted</p>
          <p style=""margin:0;font-size:12px;color:#555;"">Sep 16, 2025 - 10:00 AM</p>
        </td>
        <!-- Connector -->
        <td style=""width:5%;""><hr style=""border:none;border-top:2px solid #e0e0e0;""></td>
        <!-- Step 2 -->
        <td style=""width:33%; position:relative;"">
          <div style=""width:20px;height:20px;background:#ffc107;border-radius:50%;margin:auto;""></div>
          <p style=""margin:5px 0 0;font-weight:bold;color:#ffc107;"">Pending Manager Approval</p>
          <p style=""margin:0;font-size:12px;color:#555;"">Sep 16, 2025 - 10:05 AM</p>
        </td>
        <!-- Connector -->
        <td style=""width:5%;""><hr style=""border:none;border-top:2px solid #e0e0e0;""></td>
        <!-- Step 3 -->
        <td style=""width:33%; position:relative;"">
          <div style=""width:20px;height:20px;background:#e0e0e0;border-radius:50%;margin:auto;""></div>
          <p style=""margin:5px 0 0;font-weight:bold;color:#555;"">Leave Approved</p>
          <p style=""margin:0;font-size:12px;color:#888;"">(Waiting update)</p>
        </td>
      </tr>
    </table>
  </td>
</tr>


        <!-- Employee Info -->
        <tr>
            <td class=""content section-info"">
                <h2>Employee Information</h2>
                <table class=""info-table"" style=""margin-bottom: 10px;"">
                    <tr>
                        <th>Name</th>
                        <td>Abbas Uddin</td>
                    </tr>
                    <tr>
                        <th>Department</th>
                        <td>PHP</td>
                    </tr>
                    <tr>
                        <th>Position Title</th>
                        <td>Web Developer</td>
                    </tr>
                </table>

                <h2>Leave Details</h2>
                <table class=""info-table"">
                    <tr>
                        <th>Type of Leave</th>
                        <td>Sick Leave</td>
                    </tr>
                    <tr>
                        <th>Start Date</th>
                        <td>16-09-25</td>
                    </tr>
                    <tr>
                        <th>End Date</th>
                        <td>20-09-25</td>
                    </tr>
                    <tr>
                        <th>Reason</th>
                        <td>Medical Appointments</td>
                    </tr>
                </table>
            </td>
        </tr>





<!-- Button Info -->
<tr>
    <td class=""content section-button"">
        
        <!-- Buttons -->
        <div style=""margin-bottom: 15px; text-align: center;"">
            <a href=""https://example.com/accept?request_id=123"" 
               style=""display: inline-block; padding: 10px 20px; margin-right: 20px; background-color:#fff;border:1px solid #28a745;color:#28a745; text-decoration: none; border-radius: 5px; font-weight: bold;"">
               Accept
            </a>
            <a href=""https://example.com/deny?request_id=123"" 
               style=""display: inline-block; padding: 10px 20px; background-color: #fff; color:#dc3545;border:1px solid #dc3545; text-decoration: none; border-radius: 5px; font-weight: bold;"">
               Deny
            </a>
        </div>
		<p>If you need to modify applied date, kindly change by <a href=""#"">clicking this link</a> </p>
    </td>
</tr>
<!-- Footer -->
<tr>
  <td class=""footer section-footer"" align=""center"" style=""text-align:center;"">
    <p>© Gctlinfosys 2025. All rights reserved.</p>


  </td>
</tr>



    </table>
</body>
</html>


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
