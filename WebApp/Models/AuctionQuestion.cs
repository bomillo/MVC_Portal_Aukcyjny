using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class AuctionQuestion {
        [Key]
        public int QuestionId { get; set; }
        public User User{ get; set; }
        public Auction Auction { get; set; }
        public string Question { get; set; }
        public string? Answer { get; set; }
        public DateTime PublishedTime { get; set; }
        public DateTime AnsweredTime { get; set; }
    }
}
