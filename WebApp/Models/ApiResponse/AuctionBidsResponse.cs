namespace WebApp.Models.ApiResponse
{
    public class AuctionBidsResponse
    {
        public int AuctionId { get; set; }
        public string AuctionTitle { get; set; }
        public List<string> Bids { get; set; }
    }
}
