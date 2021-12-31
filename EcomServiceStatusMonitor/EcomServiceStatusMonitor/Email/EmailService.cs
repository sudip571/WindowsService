using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomServiceStatusMonitor.Email
{
    class EmailService
    {
       public void SendEmail(List<string> emailTo,string subject,string body)
        {
            try
            {
                string host = ConfigurationManager.AppSettings["SmtpClient"];
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                string username = ConfigurationManager.AppSettings["NetworkCredentialUsername"];
                string password = ConfigurationManager.AppSettings["NetworkCredentiaPassword"];
                string sender = ConfigurationManager.AppSettings["FromAddress"];
                string displayName = ConfigurationManager.AppSettings["DisplayNameForFromAddress"]; 

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(displayName, sender));//(Display Name, Email address)
                foreach (var item in emailTo)
                {
                    email.To.Add(new MailboxAddress("", item));
                }
                email.Subject = subject;
                var builder = new BodyBuilder();
                builder.HtmlBody = body;
                // to attach files https://github.com/taithienbo/ASPNetCoreExampleSendEmailAttachmentWithMailKit/blob/master/WebApp/Services/AppEmailService.cs
                //builder.Attachments.Add("full-file-path");
                email.Body = builder.ToMessageBody();

                using (var smtp = new SmtpClient())
                {
                    //smtp.Connect(host, port, true);
                    smtp.Connect(host, port, SecureSocketOptions.Auto);
                    smtp.Authenticate(username, password);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }

            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}
