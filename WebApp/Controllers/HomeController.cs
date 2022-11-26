using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAukcyjny.Models;
using System.Diagnostics;
using WebApp.Context;
using WebApp.Models.DTO;

namespace PortalAukcyjny.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PortalAukcyjnyContext _context;

        public HomeController(ILogger<HomeController> logger, PortalAukcyjnyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()   
        {
            var categories = (from c in _context.Categories
                             .OrderBy(cat => cat.ParentCategory)
                             .Take(10)
                             select c).ToList();

            ViewBag.Categories = categories;


            var recentlyFinished = (from a in _context.Auctions
                                    .Include(p => p.Product)
                                    .Where(x => x.EndTime <= DateTime.UtcNow)
                                    .OrderByDescending(x => x.EndTime)
                                    .Take(5)
                                    select a).ToList();

            var recentlyFinishedAuctions = new List<DisplayAuctionsModel>();

            foreach (var auction in recentlyFinished)
            {
                var Bid = _context.Bid.Where(x => x.AuctionId == auction.AuctionId).ToList();
                recentlyFinishedAuctions.Add(new DisplayAuctionsModel()
                {
                    Auction = auction,
                    Bid = Bid.Select(x => x.Price).DefaultIfEmpty(0).Max()
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
                var currentBids = _context.Bid.Where(x => x.AuctionId == auction.AuctionId).ToList();
                displayAuctions.Add(new DisplayAuctionsModel()
                {
                    Auction = auction,
                    Bid = currentBids.Select(x => x.Price).DefaultIfEmpty(0).Max()
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