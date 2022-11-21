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
        public async Task<IActionResult> Index(int page = 1)
        {
            int userId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid"))
                .Value.ToString());

            const int pageSize = 50;
            if (page < 1)
                page = 1;


            if (userId == 878)
            {
                var portalAukcyjnyContext = _context.ObservedAuctions.Include(o => o.Auction).Include(o => o.User);

                int rowsCount = portalAukcyjnyContext.Count();
                var pager = new Pager(rowsCount, page, "ObservedAuctions", pageSize);
                var rowsSkipped = (page - 1) * pageSize;
                var auctions = portalAukcyjnyContext.Skip(rowsSkipped).Take(pager.PageSize).ToList();

                ViewBag.Pager = pager;

                return View(auctions);
            }
            else
            {
                var portalAukcyjnyContext = _context.ObservedAuctions.Include(o => o.Auction).Where(u => u.UserId == userId);

                int rowsCount = portalAukcyjnyContext.Count();
                var pager = new Pager(rowsCount, page, "ObservedAuctions", pageSize);
                var rowsSkipped = (page - 1) * pageSize;
                var auctions = portalAukcyjnyContext.Skip(rowsSkipped).Take(pager.PageSize).ToList();

                ViewBag.Pager = pager;

                return View(auctions);
            }

        }

        // GET: ObservedAuctions/Details/5
        public async Task<IActionResult> Details(int? aId, int? uId)
        {
            if (aId == null || uId == null || _context.ObservedAuctions == null)
            {
                return NotFound();
            }

            var observedAuction = await _context.ObservedAuctions
                .Include(o => o.Auction)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.UserId == uId && m.AuctionId == aId);
            if (observedAuction == null)
            {
                return NotFound();
            }

            return View(observedAuction);
        }

        

        // GET: ObservedAuctions/Delete/5
        public async Task<IActionResult> Delete(int? aId, int? uId)
        {
            if (aId == null || uId == null || _context.ObservedAuctions == null)
            {
                return NotFound();
            }

            var observedAuction = await _context.ObservedAuctions
                .Include(o => o.Auction)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.UserId == uId && m.AuctionId == aId);
            if (observedAuction == null)
            {
                return NotFound();
            }

            return View(observedAuction);
        }

        // POST: ObservedAuctions/Delete/5
        public async Task<IActionResult> DeleteConfirmed(int? aId, int? uId)
        {
            if (_context.ObservedAuctions == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.ObservedAuctions'  is null.");
            }
            var observedAuction = await _context.ObservedAuctions
                .FirstOrDefaultAsync(m => m.UserId == uId && m.AuctionId == aId); 
            
            if (observedAuction != null)
            {
                _context.ObservedAuctions.Remove(observedAuction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
