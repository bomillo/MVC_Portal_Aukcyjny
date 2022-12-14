using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Services
{
    public class AuctionsService
    {
        private readonly PortalAukcyjnyContext context;

        public AuctionsService(PortalAukcyjnyContext context)
        {
            this.context = context;
        }
        public IEnumerable<Auction> GetActiveAuctions()
        {
            var auctionList = context.Auctions.Where(a => !a.IsDraft && a.PublishedTime < DateTime.UtcNow && a.EndTime > DateTime.UtcNow )
                .Include(x => x.Owner)
                .Include(x => x.Product)
                .ToList();

            return auctionList;
        } 
        public Auction GetAuction(int auctionId)
        {
            return context.Auctions.FirstOrDefault(x => x.AuctionId == auctionId);
        }
    }
}
