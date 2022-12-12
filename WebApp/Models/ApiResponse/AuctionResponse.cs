namespace WebApp.Models.ApiResponse
{
    public class AuctionResponse
    {

        public int AuctionId { get; set; }
        public string Title { get; set; }
        public string AuctionDescription { get; set; }
        public DateTime? PublishTimeUTC { get; set; }
        public DateTime? EndTimeUTC { get; set; }
        public string ProductName { get; set; }
        public string OwnerName { get; set; }
    }
}
