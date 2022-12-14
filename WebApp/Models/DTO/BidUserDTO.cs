namespace WebApp.Models.DTO
{
    public class BidUserDTO
    {
        public int BidId { get; set; }
        
        public int UserId { get; set; }
     
        public Auction Auction { get; set; }

        public string Price { get; set; }

        public string BidTime { get; set; }

        public string UserName { get; set; }
    }
}
