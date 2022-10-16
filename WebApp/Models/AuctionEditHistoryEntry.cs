namespace WebApp.Models
{
    public class AuctionEditHistoryEntry 
    {
        public int AuctionId;
        public int UserId;
        public ChangeType type;
        public DateTime ChangedTime;
        public string JsonChange;
    }

    public enum ChangeType { Name, Desc, Cośjescze };
}
