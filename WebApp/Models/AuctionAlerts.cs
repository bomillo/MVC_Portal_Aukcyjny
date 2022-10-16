using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class AuctionAlerts {
        [Key]
        public int AlertId { get; set; }
        public int UserId { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
        public double? MaxPrice { get; set; }
    }
}
