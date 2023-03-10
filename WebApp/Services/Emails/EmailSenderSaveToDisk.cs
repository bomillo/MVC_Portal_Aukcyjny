using System.Net.Mail;

namespace WebApp.Services.Emails
{
    public class EmailSenderSaveToDisk : EmailSenderDecorator
    {
        public EmailSenderSaveToDisk(IEmailSender decorated) : base(decorated) { }

        public override void SendMail(MailMessage message)
        {
            try
            {
                SmtpClient client = new SmtpClient("smtp");
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = @"mails";
                client.Send(message);
            }
            catch { }
            base.SendMail(message);
        }
    }
}