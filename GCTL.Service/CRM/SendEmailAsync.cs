using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;


namespace GCTL.Service.CRM
{

    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

    public class EmailService1
    {
        private readonly string _fromEmail = "systemtestmailuse@gmail.com";
        private readonly string _appPassword = "gfis sljk niul ycmg";

        public async Task SendEmailAsync(string toEmail, string subject, string body, byte[] attachmentBytes = null, string attachmentName = null)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.Credentials = new NetworkCredential(_fromEmail, _appPassword);
                client.EnableSsl = true;

                var mail = new MailMessage(_fromEmail, toEmail, subject, body)
                {
                    IsBodyHtml = true
                };

                if (attachmentBytes != null && attachmentName != null)
                {
                    using (var stream = new System.IO.MemoryStream(attachmentBytes))
                    {
                        mail.Attachments.Add(new Attachment(stream, attachmentName));
                        await client.SendMailAsync(mail);
                    }
                }
                else
                {
                    await client.SendMailAsync(mail);
                }
            }
        }
    }


}
