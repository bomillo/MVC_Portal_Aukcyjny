using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class AuctionEditHistoryEntry 
    {
        [Key]
        public int EntryId { get; set; }
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public ChangeType Type { get; set; }
        public DateTime ChangedTime { get; set; }
        public string JsonChange { get; set; }
    }

    public enum ChangeType { Name, Desc, Cośjescze };
}
