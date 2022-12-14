namespace WebApp.Models.ApiRequests
{
    public class AuctionBidsRequest
    {
        public int AuctionId { get; set; }
        public int? HowMany { get; set; }
    }
}
