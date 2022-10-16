using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class AuctionAlerts {
        [Key]
        public int AlertId { get; set; }
        public User User { get; set; }
        public Category? Category{ get; set; }
        public Product? Product { get; set; }
        public double? MaxPrice { get; set; }
    }
}
