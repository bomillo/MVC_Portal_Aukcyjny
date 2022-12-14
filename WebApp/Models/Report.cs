using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using WebApp.Context;

namespace WebApp.Models
{
    public abstract class Report
    {
        public string FileName { get; set; }
        private readonly PortalAukcyjnyContext dbContext;

        public Report(PortalAukcyjnyContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public void GenerateNewAuctionsReport(DateTime fromDate, int daySpan = 7)
        {
            fromDate = fromDate.ToUniversalTime();

            FileName = $"NewAuctions_{fromDate.Day}.{fromDate.Month}.{fromDate.Year}-{fromDate.Day}.{fromDate.Month}.{fromDate.Year}";
            var auctions = dbContext.Auctions.Where(x => x.CreationTime >= fromDate.ToUniversalTime() && x.CreationTime <= fromDate.ToUniversalTime().AddDays(daySpan))
                .Include(x => x.Product)
                .Include(x => x.Owner)
                .ToList();

            string[] headers = new string[]
            {
                "Auction title", "Owner name", "Owner id", "Product name", "Product id", "Creation time", "Is draft"
            };

            List<string[]> data = new List<string[]>();
            
            foreach(var auction in auctions)
            {
                data.Add(new string[]
                {
                    auction.Title, auction.Owner.Name, auction.OwnerId.ToString(), auction.Product.Name, auction.Product.ProductId.ToString(), auction.CreationTime.ToString(), auction.IsDraft.ToString()
                });
            }

            SerializeToFile(data, headers);

        }

        public void GenerateMyAuctionHistoryReport(int userId)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
            {
                return;
            }

            FileName = $"AuctionHistory_{user.Name}";

            var userAuctions = dbContext.Auctions.Where(x => x.OwnerId == userId).Include(x => x.Product).Include(x => x.Product.Category).OrderBy(x => x.CreationTime).ToList();

            

            string[] headers = new string[]
            {
                "Auction title", "Product name", "Category name", "Creation time", "Published time", "End time", "How Many Bids", "Highest Bid"
            };

            List<string[]> data = new List<string[]>();



            foreach (var auction in userAuctions)
            {
                var bidCounts = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Count();

                var highestBid = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Max(x => x.Price);

                data.Add(new string[]
                {
                    auction.Title, auction.Product.Name, auction.Product.Category.Name, auction.CreationTime.ToString(),
                    auction.PublishedTime?.ToString() ?? "Not Published Yet", auction.EndTime?.ToString() ?? "No end time defined", bidCounts.ToString(), highestBid.ToString()
                });
            }

            SerializeToFile(data, headers);

        }

        public byte[] GenerateAuctionsEndedInGivenTimeSpan(int timeSpan = 7)
        {
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);
            var endDate = DateTime.UtcNow;

            var auctions = dbContext.Auctions.Where(x => x.EndTime >= startDate && x.EndTime <= endDate)
                .Include(x => x.Product)
                .Include(x => x.Product.Category)
                .ToList();


            string[] headers = new string[]
            {
                "Auction title", "Product name", "Category name", "Creation time", "Published time", "End time", "How Many Bids", "Highest Bid"
            };

            List<string[]> data = new List<string[]>();



            foreach (var auction in auctions)
            {
                var bidCounts = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Count();

                var highestBid = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Max(x => (int?)x.Price);

                data.Add(new string[]
                {
                    auction.Title, auction.Product.Name, auction.Product.Category.Name, auction.CreationTime.ToString(),
                    auction.PublishedTime?.ToString() ?? "Not Published Yet", auction.EndTime?.ToString() ?? "No end time defined", bidCounts.ToString(), highestBid.ToString()
                });
            }

            return SerializeToFile(data, headers);

        }

        public abstract byte[] SerializeToFile(List<string[]> data, string[] headers);
    }
}
