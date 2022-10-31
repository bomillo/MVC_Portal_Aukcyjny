using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class AuctionEditHistoryEntry 
    {
        [Key]
        public int EntryId { get; set; }
        public int AuctionId { get; set; }
        public Auction Auction { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public ChangeType Type { get; set; }
        public DateTime ChangedTime { get; set; }
        [MaxLength(4000)]
        public string JsonChange { get; set; }
    }

    public enum ChangeType { Name, Desc, Cośjescze };
}
