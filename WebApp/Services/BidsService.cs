using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Net;
using System.Security.Cryptography;
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

        public async Task<List<BidDTO>> GetAuctionBids(int auctionId)
        {
            var bidList = new List<Bid>();
            //TODO replace with data from db and context
            var exchangeRate = 1;
            var currency = "zł";

            bidList = dbContext.Bid.Where(x => x.AuctionId == auctionId).OrderByDescending(x => x.Price)
                .Include(x => x.User)
                .ToList();

            var bidDTO = new List<BidDTO>();

            foreach(var bid in bidList)
            {
                bidDTO.Add(
                    new BidDTO()
                    {
                        Price = string.Format("{0:0.00} ", Math.Round(bid.Price * exchangeRate, 2)) + currency,
                        UserName = bid.User.Name.Substring(0, 3) + "..." + bid.User.Name.Substring(bid.User.Name.Length - 2, 2),
                        BidTime = bid.BidTime.ToString()
                    });
            }

            return bidDTO;
        }

        public void PlaceBid(double bid, int auctionId, int userId)
        {
            var newBid = new Bid()
            {
                AuctionId = auctionId,
                BidTime = DateTime.UtcNow,
                Price = Math.Round(bid, 2),
                UserId = userId

            };
            dbContext.Bid.Add(newBid);
            dbContext.SaveChanges();
        }

        public JsonResult ValidateBid(int auctionId, double bid, int userId)
        {
            var auction = dbContext.Auctions.Find(auctionId);
            var currentBids = dbContext.Bid.Where(x => x.AuctionId == auctionId).ToList();
            var maxBid = currentBids.Select(x => x.Price).DefaultIfEmpty(0).Max();
            var result = new JsonResult(new { });
            if (auction.OwnerId == userId)
            {
                result = new JsonResult(new { valid = false, message = WebApp.Resources.Shared.BidInvalidUser });
                result.StatusCode = 404;
                return result;
            }

            if(auction.IsDraft)
            {
                result = new JsonResult(new { valid = false, message = WebApp.Resources.Shared.AuctionIsDraft });
                result.StatusCode = 404;
                return result;
            }

            if(auction.EndTime < DateTime.UtcNow)
            {
                result = new JsonResult(new { valid = false, message = WebApp.Resources.Shared.AuctionEnded });
                result.StatusCode = 404;
                return result;
            }

            if (maxBid >= bid)
            {
                result = new JsonResult(new { valid = false, message = WebApp.Resources.Shared.BidToLow });
                result.StatusCode = 404;
                return result;
            }

            result = new JsonResult(new { valid = true });
            result.StatusCode = 200;
            return result;
        }
    }
}
