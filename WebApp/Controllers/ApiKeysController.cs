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
    public class ApiKeysController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public ApiKeysController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: ApiKeys
        public async Task<IActionResult> Index()
        {
              return View(await _context.ApiKeys.ToListAsync());
        }

        // GET: ApiKeys/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ApiKeys == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(m => m.Key == id);
            if (apiKey == null)
            {
                return NotFound();
            }

            return View(apiKey);
        }

        // GET: ApiKeys/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApiKeys/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Key")] ApiKey apiKey)
        {
            if (ModelState.IsValid)
            {
                _context.Add(apiKey);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(apiKey);
        }

        // GET: ApiKeys/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ApiKeys == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null)
            {
                return NotFound();
            }
            return View(apiKey);
        }

        // POST: ApiKeys/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Key")] ApiKey apiKey)
        {
            if (id != apiKey.Key)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apiKey);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiKeyExists(apiKey.Key))
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
            return View(apiKey);
        }

        // GET: ApiKeys/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ApiKeys == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(m => m.Key == id);
            if (apiKey == null)
            {
                return NotFound();
            }

            return View(apiKey);
        }

        // POST: ApiKeys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ApiKeys == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.ApiKeys'  is null.");
            }
            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey != null)
            {
                _context.ApiKeys.Remove(apiKey);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApiKeyExists(string id)
        {
          return _context.ApiKeys.Any(e => e.Key == id);
        }
    }
}
