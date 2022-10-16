using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Auction
    {
        public int AuctionId { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public bool IsDraft { get; set; }
        public User Owner { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime PublishedTime { get; set; }
        public DateTime EndTime { get; set; }
        public Product Product { get; set; }
    }

    public enum AuctionType { Draft, Real }
}
