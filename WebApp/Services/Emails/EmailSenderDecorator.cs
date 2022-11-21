using System.Net.Mail;

namespace WebApp.Services.Emails
{
    public class EmailSenderDecorator : IEmailSender
    {
        private IEmailSender decorated;

        public EmailSenderDecorator(IEmailSender decorated)
        {
            this.decorated = decorated;
        }

        public virtual void SendMail(MailMessage message)
        {
            decorated.SendMail(message);
        }
    }
}