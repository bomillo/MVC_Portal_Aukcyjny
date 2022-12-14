using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Text;
using WebApp.Models;
using WebApp.Models.ApiRequests;
using WebApp.Models.ApiResponse;

namespace WebApp.Services
{
    public class ApiFacadeService : IApiFacadeService
    {
        private readonly BidsService bidsService;
        private readonly AuctionsService auctionsService;
        private readonly UsersService usersService;
        private readonly ObservAuctionService observedAuctionService;
        private readonly DMService dmService;

        public ApiFacadeService(BidsService bidsService, AuctionsService auctionsService, UsersService usersService, ObservAuctionService observedAuctionService, DMService dmService)
        {
            this.bidsService = bidsService;
            this.auctionsService = auctionsService;
            this.usersService = usersService;
            this.observedAuctionService = observedAuctionService;
            this.dmService = dmService;
        }
        public ActionResult Bid(HttpContext httpContext)
        {
            httpContext.Items.TryGetValue("addBidRequest", out var request);
            if(request == null)
            {
                return new BadRequestObjectResult(new JsonResult(new { results = false, message = WebApp.Resources.Shared.NoBidValue }));

            }
            var addBidRequst = (AddBidRequset)request;

            var id = addBidRequst.AuctionId;
            var bid = addBidRequst.Bid;
            httpContext.Items.TryGetValue("userid", out var userIdObj);
            if(userIdObj == null)
            {
                return new BadRequestObjectResult(new JsonResult(new { result = false, message = WebApp.Resources.Shared.ApiAuthError }));
            }
            var userId = (int)userIdObj;
            var validationResult = bidsService.ValidateBid(id, bid, userId);
            if (validationResult.StatusCode != 200)
            {
                return new BadRequestObjectResult(validationResult);
            }
            bidsService.PlaceBid(bid, id, userId);
            return new OkObjectResult(new JsonResult(new { result = true, message = WebApp.Resources.Shared.BidPlaced }));
        }

        public ActionResult CreateNewAuction(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public ActionResult GetActiveAuctions(HttpContext httpContext)
        {

            var activeAuctions = auctionsService.GetActiveAuctions();
            List<AuctionResponse> response = new List<AuctionResponse>();
            foreach(var auction in activeAuctions)
            {
                response.Add(new AuctionResponse()
                {
                    AuctionId = auction.AuctionId,
                    Title = auction.Title,
                    AuctionDescription = auction.Description,
                    PublishTimeUTC = auction.PublishedTime,
                    EndTimeUTC = auction.EndTime,
                    ProductName = auction.Product.Name,
                    OwnerName = auction.Owner.Name
                });
            }
            return new OkObjectResult(response);
        }

        public async Task<ActionResult> GetAuctionBids(HttpContext httpContext)
        {
            httpContext.Items.TryGetValue("auctionBidsRequest", out var auctionBidsRequstObj);
            if(auctionBidsRequstObj == null)
            {
                return new BadRequestObjectResult(new JsonResult(new {message = WebApp.Resources.Shared.InvalidRequestData}));
            }
            httpContext.Items.TryGetValue("userid", out var userIdObj);
            var userId = (int)userIdObj;
            var auctionBidsRequest = (AuctionBidsRequest)auctionBidsRequstObj;
            var auction = auctionsService.GetAuction(auctionBidsRequest.AuctionId);

            if(auction == null)
            {
                return new BadRequestObjectResult(new JsonResult(new { message = WebApp.Resources.Shared.InvalidRequestData }));
            }
            else if(auction.IsDraft){
                return new BadRequestObjectResult(new JsonResult(new { message = WebApp.Resources.Shared.AuctionIsDraft }));
            }

            var bidsTask = bidsService.GetAuctionBids(auctionBidsRequest.AuctionId, userId);
            AuctionBidsResponse response = new AuctionBidsResponse();
            response.AuctionId = auctionBidsRequest.AuctionId;
            response.AuctionTitle = auction.Title;
            response.Bids = new List<string>();
            var bids = await bidsTask;
            if(auctionBidsRequest.HowMany != null && bids.Count() > auctionBidsRequest.HowMany) 
            {
                for(int i = 0; i < auctionBidsRequest.HowMany; i++)
                {
                    response.Bids.Add(bids[i].Price);
                }
            }
            else
            {
                foreach(var bid in bids)
                {
                    response.Bids.Add(bid.Price);
                }
            }


            return new OkObjectResult(response);
        }

        public ActionResult ObservedAuctions(HttpContext httpContext)
        {
            httpContext.Items.TryGetValue("userid", out var userIdObj);
            var userId = (int)userIdObj;

            var observedAuctions = usersService.GetUserObservedAuctions(userId);

            List<AuctionResponse> response = new List<AuctionResponse>();

            foreach(var observedAuction in observedAuctions)
            {
                response.Add(new AuctionResponse()
                {
                    AuctionId = observedAuction.AuctionId,
                    Title = observedAuction.Auction.Title,
                    AuctionDescription = observedAuction.Auction.Description,
                    PublishTimeUTC = observedAuction.Auction.PublishedTime,
                    EndTimeUTC = observedAuction.Auction.EndTime,
                    ProductName = observedAuction.Auction.Product.Name,
                    OwnerName = observedAuction.Auction.Owner.Name
                });
            }
            return new OkObjectResult(response);
        }

        public ActionResult SendDirectMessageToAuctionOwnerWhenAuctionNotDraftAndAuctionNotEndedAndHigherBidNotPlaced(HttpContext httpContext)
        {
            httpContext.Items.TryGetValue("userid", out var userIdObj);
            httpContext.Items.TryGetValue("createDMRequest", out var requestObj);
            if(userIdObj == null || requestObj == null)
            {
                return new BadRequestObjectResult(new JsonResult(new { message = WebApp.Resources.Shared.InvalidRequestData }));
            }
            var senderId = (int)userIdObj;
            var request = (CreateDMRequest)requestObj;

            var auction = auctionsService.GetAuction(request.AuctionId);

            var dm = new DirectMessage()
            {
                SenderId = senderId,
                ReceiverId = auction.OwnerId,
                Message = request.Message,
                SentTime = DateTime.UtcNow
            };

            dmService.SendDM(dm);

            return new OkObjectResult(new JsonResult(new { message = WebApp.Resources.Shared.MessageSent }));
        }

        public ActionResult StartObservingAuction(HttpContext httpContext)
        {
            httpContext.Items.TryGetValue("auctionId", out var auctionIdObj);
            httpContext.Items.TryGetValue("userid", out var userIdObj);

            var auctionId = (int)auctionIdObj;
            var userId = (int)userIdObj;

            observedAuctionService.Observe(auctionId, userId);
            var auction = auctionsService.GetAuction(auctionId);
            

            return new OkObjectResult(new JsonResult(new { message = $"{WebApp.Resources.Shared.NowObserving} {auction.Title}"}));
        }
    }

    public interface IApiFacadeService
    {
        public ActionResult CreateNewAuction(HttpContext httpContext);
        public ActionResult Bid(HttpContext httpContext);
        public ActionResult SendDirectMessageToAuctionOwnerWhenAuctionNotDraftAndAuctionNotEndedAndHigherBidNotPlaced(HttpContext httpContext);
        public ActionResult GetActiveAuctions(HttpContext httpContext);
        public Task<ActionResult> GetAuctionBids(HttpContext httpContext);
        public ActionResult ObservedAuctions(HttpContext httpContext);
        public ActionResult StartObservingAuction(HttpContext httpContext);
    }
}
