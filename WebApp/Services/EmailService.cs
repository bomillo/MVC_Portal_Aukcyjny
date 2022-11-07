using System.Net;
using System.Net.Mail;

namespace WebApp.Services
{
    public class EmailService
    {
        public void SendMail(string subject, string body, string to) {
            SmtpClient smtpClient = new SmtpClient();
            var basicCredential = new NetworkCredential("dominik.postolowicz@gmail.com", "8EKzNt5wIvUASjR9");
            MailAddress fromAddress = new MailAddress("no-reply@portalaukcyjny.com");
            smtpClient.EnableSsl = true;
                
            smtpClient.Host = "smtp-relay.sendinblue.com";
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = basicCredential;

            using (MailMessage message = new MailMessage())
            {
                message.From = fromAddress;
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                message.To.Add(to);

                smtpClient.Send(message);    
            }
        }
    }
}
