namespace WebApp.Models.DTO
{
    public class DisplaySingleAuctionModel
    {
        public int AuctionId { get; set; }
        public int OwnerId { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        public string EndDate { get; set; }

        public string TimeToEnd { get; set; }
        public AuctionStatus Status { get; set; }

        public List<string> Images { get; set; }

        public List<BidDTO> Bids { get; set; }

        public List<QuestionDTO> Questions { get; set; }

    }
}
