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
    [Authorize]
    public class ObservedAuctionsController : Controller
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly SetPagerService pagerService;
        private readonly UsersService usersService;

        public ObservedAuctionsController(PortalAukcyjnyContext context, SetPagerService pagerService, UsersService usersService)
        {
            _context = context;
            this.pagerService = pagerService;
            this.usersService = usersService;
        }

        // GET: ObservedAuctions
        public async Task<IActionResult> Index(int page = 1)
        {
            var usr = (HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("mail")).Value);

            User user = usersService.GetUser(usr);

            if (page < 1)
                page = 1;

            int pageSize = await pagerService.SetPager(user.UserId);

            switch(user.UserType)
            {
                case UserType.Admin:
                    {
                        var portalAukcyjnyContext = _context.ObservedAuctions.Include(o => o.Auction).Include(o => o.User);

                        int rowsCount = portalAukcyjnyContext.Count();
                        var pager = new Pager(rowsCount, page, "ObservedAuctions", pageSize);
                        var rowsSkipped = (page - 1) * pageSize;
                        var auctions = portalAukcyjnyContext.Skip(rowsSkipped).Take(pager.PageSize).ToList();

                        ViewBag.Pager = pager;

                        return View(auctions);
                    }
            
                default:
                    {

                        var portalAukcyjnyContext = _context.ObservedAuctions.Include(o => o.Auction).Where(u => u.UserId == user.UserId);

                        int rowsCount = portalAukcyjnyContext.Count();
                        var pager = new Pager(rowsCount, page, "ObservedAuctions", pageSize);
                        var rowsSkipped = (page - 1) * pageSize;
                        var auctions = portalAukcyjnyContext.Skip(rowsSkipped).Take(pager.PageSize).ToList();

                        ViewBag.Pager = pager;

                        return View(auctions);
                    }
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
