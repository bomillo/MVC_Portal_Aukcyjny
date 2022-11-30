using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class BidsController : Controller
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly BidsService bidService;

        public BidsController(PortalAukcyjnyContext context, BidsService bidService)
        {
            _context = context;
            this.bidService = bidService;
        }

        // GET: Bids
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.Bid.Include(b => b.Auction).Include(b => b.User);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: Bids/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bid == null)
            {
                return NotFound();
            }

            var bid = await _context.Bid
                .Include(b => b.Auction)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BidId == id);
            if (bid == null)
            {
                return NotFound();
            }

            return View(bid);
        }

        // GET: Bids/Create
        public IActionResult Create()
        {
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Bids/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BidId,UserId,AuctionId,BidTime,Price")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bid);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", bid.AuctionId);

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", bid.UserId);
            return View(bid);
        }

        // GET: Bids/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bid == null)
            {
                return NotFound();
            }

            var bid = await _context.Bid.FindAsync(id);
            if (bid == null)
            {
                return NotFound();
            }

            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", bid.AuctionId);

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", bid.UserId);
            return View(bid);
        }

        // POST: Bids/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BidId,UserId,AuctionId,BidTime,Price")] Bid bid)
        {
            if (id != bid.BidId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bid);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BidExists(bid.BidId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", bid.AuctionId);

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", bid.UserId);
            return View(bid);
        }

        // GET: Bids/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Bid == null)
            {
                return NotFound();
            }

            var bid = await _context.Bid
                .Include(b => b.Auction)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BidId == id);
            if (bid == null)
            {
                return NotFound();
            }

            return View(bid);
        }

        // POST: Bids/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Bid == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.Bid'  is null.");
            }
            var bid = await _context.Bid.FindAsync(id);
            if (bid != null)
            {
                _context.Bid.Remove(bid);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ValidateBid(string bidString, int auctionId)
        {
            
            if (!User.Claims.Any())
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.NotLoggedIn });
            }

            double bid;
            try {
                bidString = bidString.Replace('.', ',');
                bid = double.Parse(bidString);
            }
            catch
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.BidInvalidValue });
            }

            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value);
            var auction = await _context.Bid.FindAsync(auctionId);
            var currentBids = _context.Bid.Where(x => x.AuctionId == auctionId).ToList();
            var maxBid = currentBids.Select(x => x.Price).DefaultIfEmpty(0).Max();

            if(auction.UserId == userId)
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.BidInvalidUser });
            }
            if(maxBid >= bid)
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.BidToLow });
            }

                return new JsonResult(new { valid = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddBid(string bidString, int auctionId, string returnUrl)
        {

            double bid;
            try
            {
                bidString = bidString.Replace('.', ',');
                bid = double.Parse(bidString);
            }
            catch
            {
                bidString = bidString.Replace(',', '.');
                bid = double.Parse(bidString);
            }


            var newBid = new Bid()
            {
                AuctionId = auctionId,
                BidTime = DateTime.UtcNow,
                Price = Math.Round(bid, 2),
                UserId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value)
                
            };
            _context.Bid.Add(newBid);
            _context.SaveChanges();

            return Redirect(returnUrl);
        }

        private bool BidExists(int id)
        {
          return _context.Bid.Any(e => e.BidId == id);
        }
    }
}
