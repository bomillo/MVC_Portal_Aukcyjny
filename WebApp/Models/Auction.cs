using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Auction
    {
        [Key]
        public int AuctionId { get; set; }
        [MaxLength(2000)]
        public string Description { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [DefaultValue(true)]
        public bool IsDraft { get; set; }
        
        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? PublishedTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }

    public enum AuctionType { Draft, Real }
}
