namespace WebApp.Models.DTO
{
    public class DisplaySingleAuctionModel
    {
        public int AuctionId { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        public string EndDate { get; set; }

        public string TimeToEnd { get; set; }

        public List<string> Images { get; set; }

        public List<Bid> Bids { get; set; }

    }
}
