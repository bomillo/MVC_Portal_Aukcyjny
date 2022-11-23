using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;
using static System.Collections.Specialized.BitVector32;

namespace WebApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public UsersController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.Users.Include(u => u.Company);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Email");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Name,Email,PasswordHashed,UserType,CompanyId,ThemeType,Language")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Email", user.CompanyId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Email", user.CompanyId);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Email,PasswordHashed,UserType,CompanyId,ThemeType,Language")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Email", user.CompanyId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return _context.Users.Any(e => e.UserId == id);
        }


        // GET: Users/Details/5
        public async Task<IActionResult> UserAccount(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(m => m.UserId == id);

            ViewBag.User = user.Name;

            var myAuctions = (from a in _context.Auctions
                             .Where(au => au.OwnerId == user.UserId &&
                                   (au.EndTime > DateTime.UtcNow || 
                                    au.IsDraft == true))
                             select a).ToList();

            ViewBag.MyAuctions = myAuctions;

            var myObservedAuctions = _context.ObservedAuctions.Where(x => x.UserId == id).ToList();
            if(myObservedAuctions != null)
            {
                List<DisplayAuctionsModel> observedAuctions = new List<DisplayAuctionsModel>();

                foreach(var myObserved in myObservedAuctions)
                {
                    var Auction = _context.Auctions.Where(x => x.AuctionId == myObserved.AuctionId &&
                                                          x.IsDraft == false).FirstOrDefault();

                    if(Auction == null)
                        { continue; }

                    var Bid = _context.Bid.Where(x => x.AuctionId == myObserved.AuctionId).ToList();
                    observedAuctions.Add(new DisplayAuctionsModel()
                    {
                        Auction = Auction,
                        Bid = Bid.Select(x => x.Price).DefaultIfEmpty(0).Max()

                    });
                }
                ViewBag.MyObservedAuctions = observedAuctions;
            }

            var myBids = _context.Bid.Where(x => x.UserId == id).Include(x => x.Auction).ToList();

            ViewBag.MyBids = myBids;



            return View();

        }






    }
}
