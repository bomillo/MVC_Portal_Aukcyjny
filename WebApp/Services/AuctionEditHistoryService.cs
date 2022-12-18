using Newtonsoft.Json;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Services
{
    public class AuctionEditHistoryService
    {
        private readonly PortalAukcyjnyContext context;
        private string JsonChange;

        public AuctionEditHistoryService(PortalAukcyjnyContext context)
        {
            this.context = context;
            JsonChange = "";
        }

        public async Task AddToHistoryChange(Auction auction)
        {
            AuctionEditHistoryEntry auctionChange = new AuctionEditHistoryEntry(auction);
            
            JsonChange += JsonConvert.SerializeObject(auction);
            auctionChange.JsonChange = JsonChange;
            auctionChange.ChangedTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            auctionChange.Type = ChangeType.Cośjescze;

            context.AuctionEditHistoryEntry.Add(auctionChange);
            await context.SaveChangesAsync();
        }

        public async Task AddChangesToHistory(Auction auction, string changes)
        {
            AuctionEditHistoryEntry auctionChange = new AuctionEditHistoryEntry(auction);

            auctionChange.JsonChange = changes;
            auctionChange.ChangedTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            auctionChange.Type = ChangeType.Cośjescze;

            context.AuctionEditHistoryEntry.Add(auctionChange);
            await context.SaveChangesAsync();
        }
    }
}
