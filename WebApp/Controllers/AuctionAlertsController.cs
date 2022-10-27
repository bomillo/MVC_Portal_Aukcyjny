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
    public class AuctionAlertsController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public AuctionAlertsController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: AuctionAlerts
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.AuctionAlerts.Include(a => a.Category).Include(a => a.Product).Include(a => a.User);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: AuctionAlerts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AuctionAlerts == null)
            {
                return NotFound();
            }

            var auctionAlerts = await _context.AuctionAlerts
                .Include(a => a.Category)
                .Include(a => a.Product)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlertId == id);
            if (auctionAlerts == null)
            {
                return NotFound();
            }

            return View(auctionAlerts);
        }

        // GET: AuctionAlerts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: AuctionAlerts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlertId,UserId,CategoryId,ProductId,MaxPrice")] AuctionAlerts auctionAlerts)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auctionAlerts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", auctionAlerts.CategoryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", auctionAlerts.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionAlerts.UserId);
            return View(auctionAlerts);
        }

        // GET: AuctionAlerts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AuctionAlerts == null)
            {
                return NotFound();
            }

            var auctionAlerts = await _context.AuctionAlerts.FindAsync(id);
            if (auctionAlerts == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", auctionAlerts.CategoryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", auctionAlerts.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionAlerts.UserId);
            return View(auctionAlerts);
        }

        // POST: AuctionAlerts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlertId,UserId,CategoryId,ProductId,MaxPrice")] AuctionAlerts auctionAlerts)
        {
            if (id != auctionAlerts.AlertId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auctionAlerts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionAlertsExists(auctionAlerts.AlertId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", auctionAlerts.CategoryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", auctionAlerts.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionAlerts.UserId);
            return View(auctionAlerts);
        }

        // GET: AuctionAlerts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AuctionAlerts == null)
            {
                return NotFound();
            }

            var auctionAlerts = await _context.AuctionAlerts
                .Include(a => a.Category)
                .Include(a => a.Product)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlertId == id);
            if (auctionAlerts == null)
            {
                return NotFound();
            }

            return View(auctionAlerts);
        }

        // POST: AuctionAlerts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AuctionAlerts == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.AuctionAlerts'  is null.");
            }
            var auctionAlerts = await _context.AuctionAlerts.FindAsync(id);
            if (auctionAlerts != null)
            {
                _context.AuctionAlerts.Remove(auctionAlerts);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuctionAlertsExists(int id)
        {
          return _context.AuctionAlerts.Any(e => e.AlertId == id);
        }
    }
}
