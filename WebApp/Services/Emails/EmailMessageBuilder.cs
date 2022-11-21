using System.Net.Mail;
using System.Text;

namespace WebApp.Services.Emails
{
    public class EmailMessageBuilder
    {

        List<string> to = new();
        string from;
        string subject;

        //No hejka, co tam się z Tobą dzieje?
        //Skąd to zwątpienie?
        //Dlaczego chcesz teraz się poddać, tylko dlatego, że raz czy drugi Ci nie wyszło?
        //To nie jest żaden powód. Musisz iść i walczyć.
        //Osiągniesz cel.
        //Prędzej czy później go osiągniesz, ale musisz iść do przodu, przeć, walczyć o swoje.
        //Nie ważne, że wszystko dookoła jest przeciwko Tobie.
        //Najważniejsze jest to, że masz tutaj wole zwycięstwa.
        //To się liczy. Każdy może osiągnąć cel, nie ważne czy taki czy taki, ale trzeba iść i walczyć.
        //To teraz masz trzy sekundy żeby się otrąsnąć, powiedzieć sobie "dobra basta", pięścią w stół, idę to przodu i osiągam swój cel.
        //Pozdro.
        StringBuilder bodyBuilder = new StringBuilder();

        public MailMessage Build()
        {
            var message = new MailMessage();
            message.From = new MailAddress(from ?? "no-reply@portalaukcyjny.com"); ;
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = bodyBuilder.ToString();

            foreach (string adress in to)
            {
                message.To.Add(adress);
            }

            return message;
        }

        public EmailMessageBuilder AddToAdress(string adressTo)
        {
            to.Add(adressTo);
            return this;
        }

        public EmailMessageBuilder SetAdressFrom(string adressFrom)
        {
            from = adressFrom;
            return this;
        }
        public EmailMessageBuilder AppendToBody(string text)
        {
            bodyBuilder.AppendLine(text);
            return this;
        }
        public EmailMessageBuilder SetSubject(string subject)
        {
            this.subject = subject;
            return this;
        }
    }
}