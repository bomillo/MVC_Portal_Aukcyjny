using WebApp.Context;
using WebApp.Models;

namespace WebApp.Services
{
    public class ObservAuctionService
    {

        private readonly PortalAukcyjnyContext _context;

        public ObservAuctionService(PortalAukcyjnyContext context)
        {
            _context = context;
        }


        public ObservedAuction Observe(int auctionId, int userId)
        {
            var alreadyObserved = _context.ObservedAuctions
                .Where(oa => oa.UserId == userId && oa.AuctionId == auctionId).FirstOrDefault();

            if(alreadyObserved == null)
            {
                ObservedAuction observedAuction = new ObservedAuction(auctionId, userId);

                _context.ObservedAuctions.Add(observedAuction);
                _context.SaveChanges();

                return observedAuction;
            }
            return null;
        }
    }
}
