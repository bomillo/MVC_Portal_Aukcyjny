namespace WebApp.Models
{
    public class Bid {
        public int BidId { get; set; }
        public User User { get; set; }
        public Auction Auction { get; set; }
        public DateTime BidTime { get; set; }
        public double Price { get; set; }
    }
}
