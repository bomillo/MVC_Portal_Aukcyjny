using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;

namespace WebApp.Services
{
    public class BidsService
    {
        private readonly PortalAukcyjnyContext dbContext;

        public BidsService(PortalAukcyjnyContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<BidDTO>> GetAuctionBids(int auctionId, int userId)
        {
            var bidList = new List<Bid>();
            //TODO replace with data from db and context
            double exchangeRate = 1;
            var currency = "PLN";

            var user = dbContext.Users.Where(u => u.UserId == userId).FirstOrDefault();
            if(user != null) 
            {
                if (user.currency != "PLN")
                { 
                    currency = user.currency;
                    exchangeRate = (double)dbContext.CurrencyExchangeRates.Where(x => x.CurrencyCode == currency).FirstOrDefault()?.ExchangeRate;
                }
            }

            bidList = dbContext.Bid.Where(x => x.AuctionId == auctionId).OrderByDescending(x => x.Price)
                .Include(x => x.User)
                .ToList();

            var bidDTO = new List<BidDTO>();

            foreach(var bid in bidList)
            {
                bidDTO.Add(
                    new BidDTO()
                    {
                        Price = string.Format("{0:0.00} ", Math.Round(bid.Price / exchangeRate, 2)) + currency,
                        UserName = bid.User.Name.Substring(0, 3) + "..." + bid.User.Name.Substring(bid.User.Name.Length - 2, 2),
                        BidTime = bid.BidTime.ToString()
                    });
            }

            return bidDTO;
        }
    }
}
