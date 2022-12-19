using Microsoft.EntityFrameworkCore;
using System.Security.Policy;
using System;
using WebApp.BackgroundTasks;
using WebApp.Context;
using WebApp.Models;
using WebApp.Resources.Authentication;
using WebApp.Services.Emails;
using Elastic.Clients.Elasticsearch.Core.GetScriptContext;
using Elastic.Clients.Elasticsearch;
using static System.Formats.Asn1.AsnWriter;
using static System.Collections.Specialized.BitVector32;
using WebApp.Resources;

namespace WebApp.Services
{
    public interface IAuctionStatusNotificationService {
        public void NotifyAboutNewBid(int auctionId);
        public void NotifyAboutEndOfAuction(int auctionId);
        public void TryRegisterNewUserForNotificationOfAuction(int auctionId, int userId);
        public void RegisterNewAuctionForNotification(int auctionId);
    }

    public class AuctionsStatusService : IAuctionStatusNotificationService
    {
        private readonly IServiceScopeFactory factory;

        List<AuctionPublisher> publishers = new();
        
        public AuctionsStatusService(IServiceScopeFactory factory)
        {
            this.factory = factory;
            Initialize();
        }


        public void Initialize() {
            PortalAukcyjnyContext context = factory.CreateScope().ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();
            var auctions = context.Auctions.ToList();
            foreach (var auction in auctions)
            {
                AddEndOfAuctionListners(auction, context);
                AddBidListners(auction, context);
            }
        }

        private void AddBidListners(Auction auction, PortalAukcyjnyContext context)
        {
            if (auction.Status != AuctionStatus.Published)
            {
                return;
            }

            AuctionPublisher auctionPublisher = Get(auction.AuctionId);

            var usersToNoify = context.Bid.Include(b => b.User).Where(b => b.AuctionId == auction.AuctionId)
                                            .ToList().GroupBy(b => b.UserId)
                                            .Select(b => b.First().User)
                                            .Where(u => !u.IsFake);


            foreach (var user in usersToNoify)
            {
                auctionPublisher.Subcribe((eventType, auction) => BidAction(eventType, auction, user.UserId));
            }
        }

        private void AddEndOfAuctionListners(Auction auction, PortalAukcyjnyContext context)
        {
            if (auction.Status != AuctionStatus.Published) {
                return;
            }

            AuctionPublisher auctionPublisher = Get(auction.AuctionId);
            auctionPublisher.Subcribe(EndOfAuctionAction);
            auctionPublisher.Subcribe(UnsubscribeAction);
        }

        private void EndOfAuctionAction(AuctionEventType eventType, Auction auction)
        {
            if (eventType != AuctionEventType.AuctionEnded) {
                return;
            }

            using (var scope = factory.CreateScope())
            {
                PortalAukcyjnyContext context = factory.CreateScope().ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();
                IEmailSender emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                User? user = context.Bid.Include(b => b.User).Where(b => b.AuctionId == auction.AuctionId).ToList()
                                              .MaxBy(b=>b.Price)?.User;
                if (user is not null && !user.IsFake) {
                    var message = new EmailMessageBuilder()
                            .SetSubject(Shared.EndOfAuctionMailSubject)
                            .AppendToBody(String.Format(Shared.EndOfAuctionMailBody, auction.Title))
                            .AddToAdress(user.Email)
                            .Build();

                    emailSender.SendMail(message);
                }
            }
        }

        private void UnsubscribeAction(AuctionEventType eventType, Auction auction)
        {
            if (eventType != AuctionEventType.AuctionEnded)
            {
                return;
            }

            publishers.RemoveAll(ap=> ap.auctionId==auction.AuctionId);
        }

        private void BidAction(AuctionEventType eventType, Auction auction, int userId)
        {
            if (eventType != AuctionEventType.NewBid)
            {
                return;
            }

            using (var scope = factory.CreateScope())
            {
                PortalAukcyjnyContext context = scope.ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();
                IEmailSender emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                Bid newBid = context.Bid.Include(b => b.User)
                    .Where(b => b.AuctionId == auction.AuctionId).ToList()
                                              .MaxBy(b => b.Price)!;

                User user = context.Users.First(u=>u.UserId==userId);
                if (!user.IsFake) { 
                
                    var message = new EmailMessageBuilder()
                            .SetSubject(Shared.NewBidMailSubject)
                            .AppendToBody(String.Format(Shared.NewBidMailBody, auction.Title,newBid.Price))
                            .AddToAdress(user.Email)
                            .Build();

                    emailSender.SendMail(message);
                }
            }
        }

        private AuctionPublisher Get(int id)
        {
            try {
                return publishers.First(ap => ap.auctionId == id);
            }
            catch
            { 
                publishers.Add(new AuctionPublisher(id,factory)); 
                return publishers.First(ap => ap.auctionId == id); 
            }
        }

        public void NotifyAboutNewBid(int auctionId)
        {
            using (var scope = factory.CreateScope())
            {
                PortalAukcyjnyContext context = scope.ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();

                publishers.First(ap => ap.auctionId == auctionId).Notify(AuctionEventType.NewBid, context.Auctions.First(a => a.AuctionId == auctionId));
            }
        }

        public void TryRegisterNewUserForNotificationOfAuction(int auctionId, int userId)
        {
            using (var scope = factory.CreateScope())
            { 
                AuctionPublisher auctionPublisher = Get(auctionId);
                PortalAukcyjnyContext context = scope.ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();

    
                var alreadyRegistered = context.Bid.Include(b => b.User)
                                              .Where(b => b.AuctionId == auctionId)
                                              .ToList()
                                              .GroupBy(b => b.UserId)
                                              .Select(b => b.First().User)
                                              .Where(u => !u.IsFake)
                                              .Any(u => u.UserId == userId);

                if (!alreadyRegistered) { 
                    auctionPublisher.Subcribe((eventType, auction) => BidAction(eventType, auction, userId));
                }
            }
        }

        public void NotifyAboutEndOfAuction(int auctionId)
        {
            using (var scope = factory.CreateScope())
            {
                PortalAukcyjnyContext context = scope.ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();
             
                publishers.First(ap => ap.auctionId == auctionId).Notify(AuctionEventType.AuctionEnded, context.Auctions.First(a=>a.AuctionId == auctionId));
            }
        }

        public void RegisterNewAuctionForNotification(int auctionId)
        {
            AuctionPublisher auctionPublisher = Get(auctionId);
            auctionPublisher.Subcribe(EndOfAuctionAction);
            auctionPublisher.Subcribe(UnsubscribeAction);
        }
    }


    public class AuctionPublisher
    {
        private readonly IServiceScopeFactory factory;

        public int auctionId;
        List<Action<AuctionEventType, Auction>> listeners;

        public AuctionPublisher(int auctionId, IServiceScopeFactory factory)
        {
            this.auctionId = auctionId;
            this.listeners = new();
            this.factory = factory;
        }

        public void Subcribe(Action<AuctionEventType, Auction> listener)
        {
            listeners.Add(listener);
        }

        public void Notify(AuctionEventType eventType)
        {
            PortalAukcyjnyContext context = factory.CreateScope().ServiceProvider.GetRequiredService<PortalAukcyjnyContext>();

            Auction auction = context.Auctions.First(a => a.AuctionId == auctionId);

            Notify(eventType, auction);
        }

        public void Notify(AuctionEventType eventType, Auction auction)
        {
            foreach (var listener in listeners)
            {
                listener(eventType, auction);
            }
        }
    }

    public enum AuctionEventType
    {
        NewBid,
        AuctionEnded
    }
}
