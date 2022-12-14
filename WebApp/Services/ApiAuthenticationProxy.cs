using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Context;

namespace WebApp.Services
{
    public class ApiAuthenticationProxy : IApiFacadeService
    {
        private readonly PortalAukcyjnyContext context;
        private readonly IApiFacadeService service;


        public ApiAuthenticationProxy(PortalAukcyjnyContext _context, IApiFacadeService service)
        {
            this.context = _context;
            this.service = service;
        }

        public ActionResult Bid(HttpContext httpContext)
        {
            var authResult = AuthenticateUser(ref httpContext);


            if (!authResult)
            {
                return new BadRequestObjectResult(new JsonResult(new { result = false, message = WebApp.Resources.Shared.ApiAuthError }));
            }
            return service.Bid(httpContext);
        }

        public ActionResult CreateNewAuction(HttpContext httpContext)
        {
            var authResult = AuthenticateUser(ref httpContext);
            if (!authResult)
            {
                return new BadRequestObjectResult(new JsonResult(new { result = false, message = WebApp.Resources.Shared.ApiAuthError }));
            }

            return service.CreateNewAuction(httpContext);
        }

        public ActionResult GetActiveAuctions(HttpContext httpContext)
        {
            var authResult = AuthenticateUser(ref httpContext);
            if (!authResult)
            {
                return new BadRequestObjectResult(new JsonResult(new { result = false, message = WebApp.Resources.Shared.ApiAuthError }));
            }

            return service.GetActiveAuctions(httpContext);
        }

        public async Task<ActionResult> GetAuctionBids(HttpContext httpContext)
        {
            var authResult = AuthenticateUser(ref httpContext);
            if (!authResult)
            {
                return new BadRequestObjectResult(new JsonResult(new { result = false, message = WebApp.Resources.Shared.ApiAuthError }));
            }
            return await service.GetAuctionBids(httpContext);
        }

        public ActionResult ObservedAuctions(HttpContext httpContext)
        {
            var authResult = AuthenticateUser(ref httpContext);
            if (!authResult)
            {
                return new BadRequestObjectResult(new JsonResult(new { result = false, message = WebApp.Resources.Shared.ApiAuthError }));
            }


            return service.ObservedAuctions(httpContext);
        }

        public ActionResult SendDirectMessageToAuctionOwnerWhenAuctionNotDraftAndAuctionNotEndedAndHigherBidNotPlaced(HttpContext httpContext)
        {
            var authResult = AuthenticateUser(ref httpContext);
            if (!authResult)
            {
                return new BadRequestObjectResult(new JsonResult(new { result = false, message = WebApp.Resources.Shared.ApiAuthError }));
            }


            return service.SendDirectMessageToAuctionOwnerWhenAuctionNotDraftAndAuctionNotEndedAndHigherBidNotPlaced(httpContext);
        }

        public ActionResult StartObservingAuction(HttpContext httpContext)
        {
            var authResult = AuthenticateUser(ref httpContext);
            if (!authResult)
            {
                return new BadRequestObjectResult(new JsonResult(new { result = false, message = WebApp.Resources.Shared.ApiAuthError }));
            }


            return service.StartObservingAuction(httpContext);
        }

        private bool AuthenticateUser(ref HttpContext httpContext)
        {
            httpContext.Request.Headers.TryGetValue("apikey", out var apiKey);
            if (!context.ApiKeys.Any(x => x.Key == apiKey.ToString()))
            {
                return false;
            }

            var userId = int.Parse(apiKey.ToString().Split('-')[0]);

            var user = context.Users.Single(x => x.UserId == userId);

            httpContext.Items.Add("userid", user.UserId);

            return true;
        }
    }
}
