using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class ObservedAuction {
        public int UserId { get; set; }
        public int AuctionId { get; set; }
        public User User { get; set; }
        
        public Auction Auction { get; set; }
    }
}
