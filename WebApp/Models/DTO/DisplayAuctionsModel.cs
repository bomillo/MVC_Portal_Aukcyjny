namespace WebApp.Models.DTO
{
    public class DisplayAuctionsModel
    {

        public Auction Auction { get; set; } = new Auction();
        public string Bid { get; set; }
        public string iconPath { get; set; }
    }
}
