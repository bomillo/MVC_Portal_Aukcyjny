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
    public class DirectMessagesController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public DirectMessagesController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: DirectMessages
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.DirectMessages.Include(d => d.Receiver).Include(d => d.Sender);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: DirectMessages/Details/5
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DirectMessages == null)
            {
                return NotFound();
            }

            var directMessage = await _context.DirectMessages
                .Include(d => d.Receiver)
                .Include(d => d.Sender)
                .FirstOrDefaultAsync(m => m.MessageId == id);
            if (directMessage == null)
            {
                return NotFound();
            }

            return View(directMessage);
        }

        // GET: DirectMessages/Create
        [Authorize("RequireAdmin")]
        public IActionResult Create()
        {
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: DirectMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("MessageId,SenderId,ReceiverId,Message,SentTime")] DirectMessage directMessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(directMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId", directMessage.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId", directMessage.SenderId);
            return View(directMessage);
        }

        // GET: DirectMessages/Edit/5
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DirectMessages == null)
            {
                return NotFound();
            }

            var directMessage = await _context.DirectMessages.FindAsync(id);
            if (directMessage == null)
            {
                return NotFound();
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId", directMessage.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId", directMessage.SenderId);
            return View(directMessage);
        }

        // POST: DirectMessages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("MessageId,SenderId,ReceiverId,Message,SentTime")] DirectMessage directMessage)
        {
            if (id != directMessage.MessageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(directMessage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DirectMessageExists(directMessage.MessageId))
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
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId", directMessage.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId", directMessage.SenderId);
            return View(directMessage);
        }

        // GET: DirectMessages/Delete/5
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DirectMessages == null)
            {
                return NotFound();
            }

            var directMessage = await _context.DirectMessages
                .Include(d => d.Receiver)
                .Include(d => d.Sender)
                .FirstOrDefaultAsync(m => m.MessageId == id);
            if (directMessage == null)
            {
                return NotFound();
            }

            return View(directMessage);
        }

        // POST: DirectMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DirectMessages == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.DirectMessages'  is null.");
            }
            var directMessage = await _context.DirectMessages.FindAsync(id);
            if (directMessage != null)
            {
                _context.DirectMessages.Remove(directMessage);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DirectMessageExists(int id)
        {
          return _context.DirectMessages.Any(e => e.MessageId == id);
        }
    }
}
