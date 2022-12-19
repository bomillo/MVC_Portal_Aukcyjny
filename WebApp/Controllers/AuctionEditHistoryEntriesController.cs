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

namespace WebApp.Controllers
{
    [Authorize]
    public class AuctionEditHistoryEntriesController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public AuctionEditHistoryEntriesController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: AuctionEditHistoryEntries
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.AuctionEditHistoryEntry.Include(a => a.Auction).Include(a => a.User);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: AuctionEditHistoryEntries/Details/5
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AuctionEditHistoryEntry == null)
            {
                return NotFound();
            }

            var auctionEditHistoryEntry = await _context.AuctionEditHistoryEntry
                .Include(a => a.Auction)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.EntryId == id);
            if (auctionEditHistoryEntry == null)
            {
                return NotFound();
            }

            return View(auctionEditHistoryEntry);
        }

        // GET: AuctionEditHistoryEntries/Create
        [Authorize("RequireAdmin")]
        public IActionResult Create()
        {
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: AuctionEditHistoryEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Create([Bind("EntryId,AuctionId,UserId,Type,ChangedTime,JsonChange")] AuctionEditHistoryEntry auctionEditHistoryEntry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auctionEditHistoryEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", auctionEditHistoryEntry.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionEditHistoryEntry.UserId);
            return View(auctionEditHistoryEntry);
        }

        // GET: AuctionEditHistoryEntries/Edit/5
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AuctionEditHistoryEntry == null)
            {
                return NotFound();
            }

            var auctionEditHistoryEntry = await _context.AuctionEditHistoryEntry.FindAsync(id);
            if (auctionEditHistoryEntry == null)
            {
                return NotFound();
            }
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", auctionEditHistoryEntry.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionEditHistoryEntry.UserId);
            return View(auctionEditHistoryEntry);
        }

        // POST: AuctionEditHistoryEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("EntryId,AuctionId,UserId,Type,ChangedTime,JsonChange")] AuctionEditHistoryEntry auctionEditHistoryEntry)
        {
            if (id != auctionEditHistoryEntry.EntryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auctionEditHistoryEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionEditHistoryEntryExists(auctionEditHistoryEntry.EntryId))
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
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", auctionEditHistoryEntry.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionEditHistoryEntry.UserId);
            return View(auctionEditHistoryEntry);
        }

        // GET: AuctionEditHistoryEntries/Delete/5
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AuctionEditHistoryEntry == null)
            {
                return NotFound();
            }

            var auctionEditHistoryEntry = await _context.AuctionEditHistoryEntry
                .Include(a => a.Auction)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.EntryId == id);
            if (auctionEditHistoryEntry == null)
            {
                return NotFound();
            }

            return View(auctionEditHistoryEntry);
        }

        // POST: AuctionEditHistoryEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AuctionEditHistoryEntry == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.AuctionEditHistoryEntry'  is null.");
            }
            var auctionEditHistoryEntry = await _context.AuctionEditHistoryEntry.FindAsync(id);
            if (auctionEditHistoryEntry != null)
            {
                _context.AuctionEditHistoryEntry.Remove(auctionEditHistoryEntry);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuctionEditHistoryEntryExists(int id)
        {
          return _context.AuctionEditHistoryEntry.Any(e => e.EntryId == id);
        }
    }
}
