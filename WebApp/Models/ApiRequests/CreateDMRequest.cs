namespace WebApp.Models.ApiRequests
{
    public class CreateDMRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public int AuctionId { get; set; }
    }
}
