using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class ObservedAuctionsController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public ObservedAuctionsController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: ObservedAuctions
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.ObservedAuctions.Include(o => o.Auction).Include(o => o.User);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: ObservedAuctions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ObservedAuctions == null)
            {
                return NotFound();
            }

            var observedAuction = await _context.ObservedAuctions
                .Include(o => o.Auction)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (observedAuction == null)
            {
                return NotFound();
            }

            return View(observedAuction);
        }

        // GET: ObservedAuctions/Create
        public IActionResult Create()
        {
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: ObservedAuctions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,AuctionId")] ObservedAuction observedAuction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(observedAuction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", observedAuction.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", observedAuction.UserId);
            return View(observedAuction);
        }

        // GET: ObservedAuctions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ObservedAuctions == null)
            {
                return NotFound();
            }

            var observedAuction = await _context.ObservedAuctions.FindAsync(id);
            if (observedAuction == null)
            {
                return NotFound();
            }
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", observedAuction.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", observedAuction.UserId);
            return View(observedAuction);
        }

        // POST: ObservedAuctions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,AuctionId")] ObservedAuction observedAuction)
        {
            if (id != observedAuction.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(observedAuction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ObservedAuctionExists(observedAuction.UserId))
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
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", observedAuction.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", observedAuction.UserId);
            return View(observedAuction);
        }

        // GET: ObservedAuctions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ObservedAuctions == null)
            {
                return NotFound();
            }

            var observedAuction = await _context.ObservedAuctions
                .Include(o => o.Auction)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (observedAuction == null)
            {
                return NotFound();
            }

            return View(observedAuction);
        }

        // POST: ObservedAuctions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ObservedAuctions == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.ObservedAuctions'  is null.");
            }
            var observedAuction = await _context.ObservedAuctions.FindAsync(id);
            if (observedAuction != null)
            {
                _context.ObservedAuctions.Remove(observedAuction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ObservedAuctionExists(int id)
        {
          return _context.ObservedAuctions.Any(e => e.UserId == id);
        }
    }
}
