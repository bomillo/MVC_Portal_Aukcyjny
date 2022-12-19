using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace WebApp.Models
{
    [Index(nameof(Status), nameof(EndTime))]
    public class Auction
    {
        [Key]
        public int AuctionId { get; set; }
        [MaxLength(2000)]
        public string Description { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        public AuctionStatus Status { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? PublishedTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }

    public enum AuctionStatus { Draft = 0, Published = 1, Ended = 2}
}
