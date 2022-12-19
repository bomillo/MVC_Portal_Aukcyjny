using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAukcyjny.Models;
using System.Diagnostics;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;
using WebApp.Services;

namespace PortalAukcyjny.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PortalAukcyjnyContext _context;
        private readonly AuctionFilesService filesService;
        private readonly BidsService bidsService;
        private string iconErrorPath;
        private string imageErrorPath;

        public HomeController(ILogger<HomeController> logger, 
            PortalAukcyjnyContext context, 
            AuctionFilesService filesService,
            BidsService bidsService)
        {
            _logger = logger;
            _context = context;
            this.filesService = filesService;
            this.bidsService = bidsService;
            iconErrorPath = filesService.GetErrorIconPath();
            imageErrorPath = filesService.GetErrorImagePath();
        }

        public async Task<IActionResult> IndexAsync()   
        {
            var categories = (from c in _context.Categories
                             .Take(10)
                             select c).ToList();

            ViewBag.Categories = categories;
            int userId;
            int.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid"))?.Value, out userId);


            var recentlyFinished = (from a in _context.Auctions
                                    .Include(p => p.Product)
                                    .Where(x => x.EndTime <= DateTime.UtcNow && x.IsDraft == false)
                                    .OrderByDescending(x => x.EndTime)
                                    .Take(5)
                                    select a).ToList();

            var recentlyFinishedAuctions = new List<DisplayAuctionsModel>();

            foreach (var auction in recentlyFinished)
            {
                string path = null;
                var image = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("IMAGE")).FirstOrDefault();

                if (image != null)
                    path = image.Path;
                else
                    path = imageErrorPath;

                var Bid = bidsService.GetAuctionHighestBid(auction.AuctionId, userId);
                recentlyFinishedAuctions.Add(new DisplayAuctionsModel()
                {
                    Auction = auction,
                    iconPath = path,
                    Bid = Bid
                });
            }

            ViewBag.RecentlyFinishedAuctions = recentlyFinishedAuctions;


            var interestingAuctions = (from a in _context.Auctions
                                       .Include(p => p.Product)
                                       .Where(au => au.IsDraft == false && 
                                              DateTime.UtcNow < au.EndTime  && 
                                              au.EndTime < DateTime.UtcNow.AddDays(7))
                                        .Take(9)
                                        select a).ToList();

            var displayAuctions = new List<DisplayAuctionsModel>();

            foreach (var auction in interestingAuctions)
            {
                var icon = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("ICON")).FirstOrDefault();
                var Bid = bidsService.GetAuctionHighestBid(auction.AuctionId, userId);
                string path = null;
                
                if (icon != null)
                    path = icon.Path;
                else
                    path = iconErrorPath;

                displayAuctions.Add(new DisplayAuctionsModel()
                {
                    Auction = auction,
                    iconPath = path,
                    Bid = Bid 
                });

            }

            ViewBag.InterestingAuctions = displayAuctions;


            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}