namespace WebApp.Models.DTO
{
    public class DisplayAuctionsModel
    {

        public Auction Auction { get; set; } = new Auction();
        public double Bid { get; set; }
    }
}
