using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Bid {
        [Key]
        public int BidId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int Auctionid { get; set; }
        public Auction Auction { get; set; }
        public DateTime BidTime { get; set; }
        [Required]
        public double Price { get; set; }
    }
}
