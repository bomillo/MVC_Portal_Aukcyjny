﻿using System.Net;
using System.Net.Mail;

namespace WebApp.Services.Emails
{
    public interface IEmailSender
    {
        public void SendMail(MailMessage message);
    }

    public class EmailSender : IEmailSender
    {
        public void SendMail(MailMessage message)
        {
            SmtpClient smtpClient = new SmtpClient();
            var basicCredential = new NetworkCredential("***REMOVED***", "***REMOVED***");
            smtpClient.EnableSsl = true;

            smtpClient.Host = "smtp-relay.sendinblue.com";
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = basicCredential;

            smtpClient.Send(message);
        }
    }
}