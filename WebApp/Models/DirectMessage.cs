using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class DirectMessage 
    {
        [Key]
        public int MessageId { get; set; }
        public User Sender { get; set; }
        public User Receiver { get; set; }
        public string Message { get; set; }
        public DateTime SentTime { get; set; }
    }
}
