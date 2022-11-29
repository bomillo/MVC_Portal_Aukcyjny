using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class DirectMessage 
    {
        [Key]
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int ReceiverId { get; set; }
        public User Receiver { get; set; }
        [MaxLength(2000)]
        public string Message { get; set; }
        public DateTime SentTime { get; set; }
        public bool Recieved { get; set; }
    }
}
