using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using WebApp.Context;

namespace WebApp.Models
{
    public abstract class Report
    {

        private readonly PortalAukcyjnyContext dbContext;

        public Report(PortalAukcyjnyContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public byte[] GenerateNewAuctionsReport(int daySpan = 3)
        {
            var fromDate = DateTime.UtcNow.AddDays(daySpan * -1);
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

            return SerializeToFile(ConvertToFileFormat(data, headers));

        }

        public void GenerateMyAuctionHistoryReport(int userId)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
            {
                return;
            }

            var userAuctions = dbContext.Auctions.Where(x => x.OwnerId == userId).Include(x => x.Product).Include(x => x.Product.Category).OrderBy(x => x.CreationTime).ToList();

            

            string[] headers = new string[]
            {
                "Auction title", "Product name", "Category name", "Creation time", "Published time", "End time", "How Many Bids", "Highest Bid"
            };

            List<string[]> data = new List<string[]>();



            foreach (var auction in userAuctions)
            {
                var bidCounts = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Count();

                var highestBid = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Max(x => (double?)x.Price);

                data.Add(new string[]
                {
                    auction.Title, auction.Product.Name, auction.Product.Category.Name, auction.CreationTime.ToString(),
                    auction.PublishedTime?.ToString() ?? "Not Published Yet", auction.EndTime?.ToString() ?? "No end time defined", bidCounts.ToString(), String.Format("{0:0.00}", highestBid != null ? highestBid : 0.00)
                });
            }

            SerializeToFile(ConvertToFileFormat(data, headers));

        }

        public byte[] GenerateAuctionsEndedInGivenTimeSpan(int timeSpan = 3)
        {
            var fromDate = DateTime.UtcNow.AddDays(timeSpan * -1);
            var toDate = DateTime.UtcNow;
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

                var highestBid = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Max(x => (double?)x.Price);

                data.Add(new string[]
                {
                    auction.Title, auction.Product.Name, auction.Product.Category.Name, auction.CreationTime.ToString(),
                    auction.PublishedTime?.ToString() ?? "Not Published Yet", auction.EndTime?.ToString() ?? "No end time defined", bidCounts.ToString(), String.Format("{0:0.00}", highestBid != null ? highestBid : 0.00)
                });
            }

            return SerializeToFile(ConvertToFileFormat(data, headers));
        }

        public byte[] GenerateCategoryPopularityInDaySpan(int daySpan = 3)
        {
            var fromDate = DateTime.UtcNow.AddDays(daySpan * -1);
            var toDate = DateTime.UtcNow;

            var bidsInCategory = dbContext.Bid.Where(x => x.BidTime >= fromDate && x.BidTime <= toDate)
                .Include( x => x.Auction)
                .Include(x => x.Auction.Product)
                .GroupBy(x => x.Auction.Product.CategoryId).Select(g => new
                {
                    CategoryId = g.Key,
                    Occurance = g.Count()
                }).OrderByDescending(a => a.Occurance).ToList();

            string[] headers = new string[]
            {
                "Category id", "Category Name", "How Many Bids"
            };
            List<string[]> data = new List<string[]>();

            foreach (var cat in bidsInCategory)
            {
                var category = dbContext.Categories.Single(x => x.CategoryId == cat.CategoryId);

                data.Add(new string[]
                {
                    cat.CategoryId.ToString(), category.Name, cat.Occurance.ToString()
                });
            }

            return SerializeToFile(ConvertToFileFormat(data, headers));
        }

        public byte[] GenerateBusinessReport(int daySpan = 3)
        {
            var fromDate = DateTime.UtcNow.AddDays(daySpan * -1);
            var toDate = DateTime.UtcNow;

            var auctionsEnded = dbContext.Auctions.Where(x => x.EndTime >= fromDate && x.EndTime <= toDate)
                .Include(p => p.Product)
                .Include(p => p.Product.Category)
                .Include(o => o.Owner)
                .ToList();

            var bids = dbContext.Bid.Where(b => b.Auction.EndTime <= toDate && b.Auction.EndTime >= fromDate)
                .GroupBy(a => a.AuctionId)
                .Select(y => new
                {
                    auctionId = y.Key,
                    Price = y.Max(a => a.Price)
                }).ToList();

            var sum = bids.Sum(x => x.Price);

            var newAuctions = dbContext.Auctions.Where(x => x.CreationTime <= toDate && x.CreationTime >= fromDate).Include(x => x.Product).Include(x => x.Product.Category).Include(o => o.Owner).ToList();
            /*
             .Sum(group => group.Max(x => (double?)x.Price));
             */
            var titles = new string[]
            {
                "Report period start","Report period end" ,"How Many auctions ended", "How Many new Auctions", "Winning Bids Sumary"
            };

            var mainData = new List<string[]>();

            mainData.Add(new string[]
            {
                fromDate.ToString(), toDate.ToString(), auctionsEnded.Count().ToString(), newAuctions.Count().ToString(), String.Format("{0:0.00}", sum)
            });

            var mainStr = ConvertToFileFormat(mainData, titles);

            string[] headers = new string[]
            {
                "Auction title", "Product name", "Category name", "Creation time", "Published time", "End time"
            };

            List<string[]> data = new List<string[]>();

            

            foreach (var auction in newAuctions)
            {
                var bidCounts = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Count();

                var highestBid = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Max(x => (double?)x.Price);

                data.Add(new string[]
                {
                    auction.Title, auction.Product.Name, auction.Product.Category.Name, auction.CreationTime.ToString(),
                    auction.PublishedTime?.ToString() ?? "Not Published Yet", auction.EndTime?.ToString() ?? "No end time defined"
                });
            }

            var newAuctionsStr = ConvertToFileFormat(data, headers);


            string[] headers2 = new string[]
            {
                "Auction title", "Product name", "Category name", "Creation time", "Published time", "End time", "How Many bids", "highest bid"
            };

            List<string[]> data2 = new List<string[]>();


            foreach (var auction in auctionsEnded)
            {
                var bidCounts = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Count();

                var highestBid = dbContext.Bid.Where(x => x.AuctionId == auction.AuctionId).Max(x => (double?)x.Price);

                data2.Add(new string[]
                {
                    auction.Title, auction.Product.Name, auction.Product.Category.Name, auction.CreationTime.ToString(),
                    auction.PublishedTime?.ToString() ?? "Not Published Yet", auction.EndTime?.ToString() ?? "No end time defined", bidCounts.ToString(), String.Format("{0:0.00}", highestBid != null ? highestBid : 0.00)
                });
            }

            var endedAuctionsStr = ConvertToFileFormat(data2, headers2);


            StringBuilder builder = new StringBuilder();
            builder.AppendLine(mainStr);
            builder.AppendLine();
            builder.AppendLine(newAuctionsStr);
            builder.AppendLine();
            builder.AppendLine(endedAuctionsStr);

            return SerializeToFile(builder.ToString());

        }

        public abstract string ConvertToFileFormat(List<string[]> data, string[] headers);

        public abstract byte[] SerializeToFile(string fileData);
    }
}
