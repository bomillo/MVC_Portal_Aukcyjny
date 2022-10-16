namespace WebApp.Models
{
    public class Bid {
        public int BidId { get; set; }
        public int UserId { get; set; }
        public int AuctionId { get; set; }
        public DateTime BidTime { get; set; }
        public double Price { get; set; }
    }
}
