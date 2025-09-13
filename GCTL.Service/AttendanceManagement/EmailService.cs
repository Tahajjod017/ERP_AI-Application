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


        public EmailService(IGenericRepository<EmailSettings> genericRepository, IGenericRepository<EmailSettings> emailSettings, IConfiguration configuration) : base(genericRepository)
        {
            _emailSettings = emailSettings;
            _configuration = configuration;
        }


        public async Task SendEmailAsync(EmailVM model)
        {
            // get first (or only) settings record
            var emailConfig = await _emailSettings.AllActive().FirstOrDefaultAsync();
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
                    Subject = model.Subject ?? "No Subject",
                    Body = string.IsNullOrEmpty(model.Body) ? BuildEmailBody(model) : model.Body,
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        private string BuildEmailBody(EmailVM model)
        {
            return $"<p>তারিখ: {DateTime.Now.ToBanglaDate()}</p>";
        }


        #region SendEmailAsync
        //public async Task SendEmailAsync(EmailVM model)
        //{
        //    var emailConfig = _configuration.GetSection("Email");

        //    string? host = emailConfig["host"];
        //    int port = int.Parse(emailConfig["port"] ?? "0");
        //    string? mailFrom = emailConfig["mailFrom"];
        //    bool enableSsl = bool.Parse(emailConfig["enableSsl"] ?? "false");
        //    bool useDefaultCredentials = bool.Parse(emailConfig["useDefaultCredentials"] ?? "false");
        //    string? userName = emailConfig["userName"];
        //    string? password = emailConfig["password"];

        //    if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(mailFrom) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        //    {
        //        throw new InvalidOperationException("Email configuration is missing or incomplete.");
        //    }

        //    string subject = "ব্যাংক হিসাব সম্পর্কিত তথ্য প্রদান।";

        //    // Build the email body
        //    string body = BuildEmailBody(model);

        //    using (var smtpClient = new SmtpClient(host, port))
        //    {
        //        smtpClient.EnableSsl = enableSsl;
        //        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        smtpClient.UseDefaultCredentials = useDefaultCredentials;

        //        if (!useDefaultCredentials)
        //        {
        //            smtpClient.Credentials = new NetworkCredential(userName, password);
        //        }

        //        var email = model.To ?? "rhsrakib030@gmail.com";
        //        if (email == null)
        //        {
        //            throw new ArgumentNullException(nameof(model.To), "Recipient email address is required.");
        //        }

        //        var mailMessage = new MailMessage(mailFrom, email) // model.Email
        //        {
        //            Subject = subject,
        //            Body = body,
        //            IsBodyHtml = true
        //        };

        //        try
        //        {
        //            await smtpClient.SendMailAsync(mailMessage);
        //            // Log success or perform additional actions if needed
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new InvalidOperationException("Failed to send email.", ex);
        //        }
        //    }
        //}
        #endregion


        #region BuildEmailBody
        //private string BuildEmailBody(EmailVM model)
        //{
        //    return $@"
        //        <!DOCTYPE html>
        //        <html lang='bn'>
        //        <head>
        //            <meta charset='UTF-8'>
        //            <title>Email</title>
        //            <style>
        //                body {{
        //                    font-family: 'Siyam Rupali', 'SolaimanLipi', Arial, sans-serif;
        //                    margin: 50px;
        //                    line-height: 1.8;
        //                }}
        //                .center {{
        //                    text-align: center;
        //                }}
        //                .content {{
        //                    margin-top: 30px;
        //                }}
        //            </style>
        //        </head>
        //        <body>
        //            <p>তারিখ: {DateTime.Now.ToBanglaDate()}</p>
        //        </body>
        //        </html>
        //    ";
        //}
        #endregion
    }
}
