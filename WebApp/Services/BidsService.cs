using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;

namespace WebApp.Services
{
    public class BidsService
    {
        private readonly PortalAukcyjnyContext dbContext;
        private double exchangeRate = 1;
        private string currency = "PLN";


        public BidsService(PortalAukcyjnyContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<BidDTO>> GetAuctionBids(int auctionId, int userId)
        {
            var bidList = new List<Bid>();
            //TODO replace with data from db and context

            setUserCurrency(userId);

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

        private void setUserCurrency(int userId)
        {
            exchangeRate = 1;
            currency = "PLN";

            var user = dbContext.Users.Where(u => u.UserId == userId).FirstOrDefault();
            if (user != null)
            {
                if (user.currency != "PLN")
                {
                    currency = user.currency;
                    exchangeRate = (double)dbContext.CurrencyExchangeRates.Where(x => x.CurrencyCode == currency).FirstOrDefault()?.ExchangeRate;
                }
            }
        }

        public string GetAuctionHighestBid(int auctionId, int userId)
        {
            setUserCurrency(userId);

            var bid = dbContext.Bid.Where(x => x.AuctionId == auctionId).OrderByDescending(x => x.Price).FirstOrDefault();

            if (bid != null) 
                return string.Format("{0:0.00} ", Math.Round(bid.Price / exchangeRate, 2)) + currency;
            else 
                return string.Format("{0:0.00} ", "0,00") + currency;
        }

        public List<BidUserDTO> GetUserBids(int userId)
        {
            setUserCurrency(userId);

            var bidList = dbContext.Bid.Where(x => x.UserId == userId).OrderByDescending(x => x.Price).Include(x => x.Auction).ToList();

            var bidUserDTO = new List<BidUserDTO>();

            foreach (var bid in bidList)
            {
                bidUserDTO.Add(
                    new BidUserDTO()
                    {
                        Price = string.Format("{0:0.00} ", Math.Round(bid.Price / exchangeRate, 2)) + currency,
                        UserName = bid.User.Name.Substring(0, 3) + "..." + bid.User.Name.Substring(bid.User.Name.Length - 2, 2),
                        BidTime = bid.BidTime.ToString(),
                        BidId = bid.BidId,
                        Auction = bid.Auction,
                        UserId = bid.UserId
                    });
            }

            return bidUserDTO;
        }
    }
}
