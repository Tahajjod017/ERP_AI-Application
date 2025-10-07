using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RazorLight;
using GCTL.Data.Models;
using GCTL.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.EmailServicesMethod
{
    public class EmailService :IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly RazorLightEngine _razorEngine;
        private readonly IGenericRepository<EmailSettings> _EmailConfig;

        public EmailService(IConfiguration configuration, IGenericRepository<EmailSettings> emailConfig)
        {
            _configuration = configuration;

            _razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EmailTemplates"))
                .UseMemoryCachingProvider()
                .Build();
            _EmailConfig = emailConfig;
        }

        public async Task<string> SendEmailAsync(
            string toEmail,
            string subject,
            string razorTemplateFile,
            object model,
            byte[]? attachment = null,
            string? attachmentFileName = null)
        {
            // Retrieve the active email configuration from the database
            var emailConfigg = await _EmailConfig.AllActive()
                .Where(es => es.IsActive)
                .OrderByDescending(es => es.PriorityIndex) // Use the priority if you have multiple entries
                .FirstOrDefaultAsync();

            if (emailConfigg == null)
            {
                return "Email configuration is missing or inactive.";
            }
            // Use the settings from the database
            string host = emailConfigg.Host;
            int port = emailConfigg.Port;
            string mailFrom = emailConfigg.MailFrom;
            bool enableSsl = emailConfigg.IsSSLRequired;
            bool useDefaultCredentials = emailConfigg.IsDefaultCredential;
            string userName = emailConfigg.UserName;
            string password = emailConfigg.Password;
            //var emailConfig = _configuration.GetSection("Email");

            //string? host = emailConfig["host"];
            //int port = int.Parse(emailConfig["port"] ?? "0");
            //string? mailFrom = emailConfig["mailFrom"];
            //bool enableSsl = bool.Parse(emailConfig["enableSsl"] ?? "false");
            //bool useDefaultCredentials = bool.Parse(emailConfig["useDefaultCredentials"] ?? "false");
            //string? userName = emailConfig["userName"];
            //string? password = emailConfig["password"];

            //if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(mailFrom) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            //{
            //    return "Email configuration is missing required fields.";
            //}

            // Load Razor template and inject model
            string htmlBody;
            //Add PreserveCompilationContext in your .csproj file
            try
            {
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EmailTemplates", razorTemplateFile);//add it <PreserveCompilationContext>true</PreserveCompilationContext> on <PropertyGroup> on EditProject
                string template = await File.ReadAllTextAsync(templatePath);
                htmlBody = await _razorEngine.CompileRenderStringAsync(razorTemplateFile, template, model);
            }
            catch (Exception ex)
            {
                return $"Error rendering template: {ex.Message}";
            }

            using var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = useDefaultCredentials,
                Credentials = useDefaultCredentials ? null : new NetworkCredential(userName, password)
            };

            using var mailMessage = new MailMessage(mailFrom, toEmail)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            if (attachment != null && !string.IsNullOrEmpty(attachmentFileName))
            {
                mailMessage.Attachments.Add(new Attachment(new MemoryStream(attachment), attachmentFileName));
            }

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return "Email sent successfully!";
            }
            catch (Exception ex)
            {
                return $"Email sending failed: {ex.Message}";
            }
        }

    }
}
