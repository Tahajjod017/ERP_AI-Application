//using GCTL.Service.AttendanceManagement;
//using System.Net.Mail;
//using System.Net.Mime;

//namespace GCTL.Service.CRM
//{
//    public class SendEmailAsync
//    {
//        private readonly EmailService _emailService;

//        public SendEmailAsync(EmailService emailService)
//        {
//            _emailService = emailService;
//        }

//        public async Task SendAsync(
//            int organizationId,
//            string toEmail,
//            string subject,
//            string body,
//            byte[]? attachmentBytes = null,
//            string? attachmentName = null,
//            List<LinkedResource>? linkedResources = null)
//        {
//            var emailConfig = await _emailService.GetEmailConfigAsync(organizationId);
//            using (var client = await _emailService.GetSmtpClientAsync(organizationId))
//            {
//                using (var mail = new MailMessage(emailConfig.UserName, toEmail))
//                {
//                    mail.Subject = subject;
//                    mail.IsBodyHtml = true;

//                    if (linkedResources != null && linkedResources.Any())
//                    {
//                        var htmlView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
//                        foreach (var resource in linkedResources)
//                            htmlView.LinkedResources.Add(resource);
//                        mail.AlternateViews.Add(htmlView);
//                    }
//                    else
//                    {
//                        mail.Body = body;
//                    }

//                    if (attachmentBytes != null && !string.IsNullOrEmpty(attachmentName))
//                    {
//                        using (var stream = new MemoryStream(attachmentBytes))
//                        {
//                            mail.Attachments.Add(new Attachment(stream, attachmentName));
//                            await client.SendMailAsync(mail);
//                        }
//                    }
//                    else
//                    {
//                        await client.SendMailAsync(mail);
//                    }
//                }
//            }
//        }

//    }
//}
