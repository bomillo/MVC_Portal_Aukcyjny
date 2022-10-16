using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class DirectMessage 
    {
        [Key]
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int RecieverId { get; set; }
        public string Message { get; set; }
        public DateTime SentTime { get; set; }
    }
}
