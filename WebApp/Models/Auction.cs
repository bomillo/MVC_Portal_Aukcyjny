namespace WebApp.Models
{
    public class Auction
    {
        private int auctionId;
        private int productId;
        private string title;
        private bool isDraft;
        private string description;
        private string auctionName;
        public int OwnerUserId;
        public DateTime CreationTime;
        public DateTime PublishedTime;
        public DateTime EndTime;

        public int AuctionId { get => auctionId; set => auctionId = value; }
        public string Description { get => description; set => description = value; }
    }

    public enum AuctionType { Draft, Real }
}
