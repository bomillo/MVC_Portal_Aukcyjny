using WebApp.Context;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.BackgroundTasks
{
    public class AuctionStatusWorker : BackgroundService
    {
        private readonly IServiceScopeFactory factory;
        private readonly AuctionsStatusService service;

        public AuctionStatusWorker(AuctionsStatusService service, IServiceScopeFactory factory)
        {
            this.service = service;
            this.factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                UpdateAuctions();
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private void UpdateAuctions()
        {
            using (var scope = factory.CreateScope())
            {
                PortalAukcyjnyContext context = scope.ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();

                var auctionsToEnd = context.Auctions.Where(a => a.Status == AuctionStatus.Published && a.EndTime < DateTime.UtcNow).ToList();

                foreach (var auction in auctionsToEnd)
                {
                    auction.Status = AuctionStatus.Ended;
                    context.Update(auction);
                }
                context.SaveChanges();

                foreach (var auction in auctionsToEnd)
                {
                    service.NotifyAboutEndOfAuction(auction.AuctionId);
                }
            }
        }
    }
}
