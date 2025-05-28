using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RazorLight;

namespace GCTL_App.EmailServicesMethod
{
    public class EmailService :IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly RazorLightEngine _razorEngine;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            _razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "EmailTemplates"))
                .UseMemoryCachingProvider()
                .Build();  
        }

        public async Task<string> SendEmailAsync(
            string toEmail,
            string subject,
            string razorTemplateFile,
            object model,
            byte[]? attachment = null,
            string? attachmentFileName = null)
        {
            var emailConfig = _configuration.GetSection("Email");

            string? host = emailConfig["host"];
            int port = int.Parse(emailConfig["port"] ?? "0");
            string? mailFrom = emailConfig["mailFrom"];
            bool enableSsl = bool.Parse(emailConfig["enableSsl"] ?? "false");
            bool useDefaultCredentials = bool.Parse(emailConfig["useDefaultCredentials"] ?? "false");
            string? userName = emailConfig["userName"];
            string? password = emailConfig["password"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(mailFrom) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return "Email configuration is missing required fields.";
            }

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
