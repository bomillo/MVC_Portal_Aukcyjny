using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Security.Claims;
using WebApp.Context;
using WebApp.Models.ApiRequests;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class ApiController : Controller
    {
        private readonly ApiAuthenticationProxy proxy;

        public ApiController(ApiAuthenticationProxy proxy)
        {
            this.proxy = proxy;
        }

        [HttpGet]
        [Route("/api/Auctions/List")]
        public ActionResult ListActiveAuctions()
        {
            return proxy.GetActiveAuctions(HttpContext);
        }

        [HttpGet]
        [Route("/api/Auctions/Bids")]
        public async Task<ActionResult> ListAuctionBids(AuctionBidsRequest auctionBidsRequest)
        {
            if (auctionBidsRequest.AuctionId == 0)
            {
                return new BadRequestObjectResult(new JsonResult(new { message = WebApp.Resources.Shared.InvalidRequestData }));
            }

            HttpContext.Items.Add("auctionBidsRequest", auctionBidsRequest);
            return await proxy.GetAuctionBids(HttpContext);
        }

        [HttpGet]
        [Route("/api/User/ObservedAuctions/List")]
        public ActionResult MyObservedAuctions()
        {
            return proxy.ObservedAuctions(HttpContext);
        }

        [HttpPost]
        [Route("/api/Bid/Add")]
        public ActionResult Bid(AddBidRequset addBidRequset)
        {
            if (addBidRequset.Bid == 0 || addBidRequset.AuctionId == 0)
            {
                return new BadRequestObjectResult(new JsonResult(new { message = WebApp.Resources.Shared.InvalidRequestData }));
            }
            HttpContext.Items.Add("addBidRequest", addBidRequset);
            return proxy.Bid(HttpContext);
        }

        

        [HttpPost]
        [Route("/api/User/ObservedAuctions/Observe")]
        public ActionResult StartObservingAuction(int auctionId)
        {
            if (auctionId == 0)
            {
                return new BadRequestObjectResult(new JsonResult(new { message = WebApp.Resources.Shared.InvalidRequestData }));
            }
            HttpContext.Items.Add("auctionId", auctionId);
            return proxy.StartObservingAuction(HttpContext);
        }

        [HttpPost]
        [Route("/api/user/directmessage/new")]
        public ActionResult DirectMessage(CreateDMRequest request)
        {
            if(string.IsNullOrEmpty(request.Message) || string.IsNullOrWhiteSpace(request.Title) || request.AuctionId == 0)
            {
                return new BadRequestObjectResult(new JsonResult(new { message = WebApp.Resources.Shared.InvalidRequestData }));
            }
            HttpContext.Items.Add("createDMRequest", request);
            return proxy.SendDirectMessageToAuctionOwnerWhenAuctionNotDraftAndAuctionNotEndedAndHigherBidNotPlaced(HttpContext);
        }
    }
}
