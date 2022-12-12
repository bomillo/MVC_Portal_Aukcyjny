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

        // returns true if auction is no longer being observed
        // and false if something went wrong
        public bool UnObserve(int auctionId, int userId)
        {
            var alreadyObserved = _context.ObservedAuctions
                .Where(oa => oa.UserId == userId && oa.AuctionId == auctionId).FirstOrDefault();

            if (alreadyObserved != null)
            {
                _context.ObservedAuctions.Remove(alreadyObserved);
                _context.SaveChanges();

                return true;
            }

            return false;
        }

        public bool IsAuctionObserved(int auctionId, int userId)
        {
            var alreadyObserved = _context.ObservedAuctions
                .Where(oa => oa.UserId == userId && oa.AuctionId == auctionId).FirstOrDefault();

            if (alreadyObserved != null)
            {
                return true;
            }
            return false;
        }
    }
}
