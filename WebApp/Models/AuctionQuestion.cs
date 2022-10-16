namespace WebApp.Models
{
    public class AuctionQuestion {
        public int UserId;
        public int AuctionId;
        public string Question;
        public string? Answer;
        public DateTime PublishedTime;
        public DateTime AnsweredTime;
    }
}
