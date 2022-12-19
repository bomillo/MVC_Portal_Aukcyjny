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
    [Authorize("RequireAdmin")]
    public class CurrencyExchangeRatesController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public CurrencyExchangeRatesController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: CurrencyExchangeRates
        public async Task<IActionResult> Index()
        {
            var rates = await _context.CurrencyExchangeRates
                .OrderByDescending(x => x.LastUpdatedTime).Take(34).ToListAsync();  // 34 - total number of currencies

            return View(rates);
        }

        // GET: CurrencyExchangeRates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CurrencyExchangeRates == null)
            {
                return NotFound();
            }

            var currencyExchangeRate = await _context.CurrencyExchangeRates
                .FirstOrDefaultAsync(m => m.CurrencyId == id);

            if (currencyExchangeRate == null)
            {
                return NotFound();
            }
            
            var currHistory = _context.CurrencyExchangeRates
                .Where(x => x.CurrencyCode == currencyExchangeRate.CurrencyCode)
                .OrderByDescending(x => x.LastUpdatedTime).ToList();

            return View(currHistory);
        }

        // GET: CurrencyExchangeRates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CurrencyExchangeRates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CurrencyId,CurrencyCode,CurrencyExchangeRate,LastUpdatedTime")] CurrencyExchangeRate currencyExchangeRate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(currencyExchangeRate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(currencyExchangeRate);
        }

        // GET: CurrencyExchangeRates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CurrencyExchangeRates == null)
            {
                return NotFound();
            }

            var currencyExchangeRate = await _context.CurrencyExchangeRates.FindAsync(id);
            if (currencyExchangeRate == null)
            {
                return NotFound();
            }
            return View(currencyExchangeRate);
        }

        // POST: CurrencyExchangeRates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CurrencyId,CurrencyCode,CurrencyExchangeRate,LastUpdatedTime")] CurrencyExchangeRate currencyExchangeRate)
        {
            if (id != currencyExchangeRate.CurrencyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(currencyExchangeRate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrencyExchangeRateExists(currencyExchangeRate.CurrencyId))
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
            return View(currencyExchangeRate);
        }

        // GET: CurrencyExchangeRates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CurrencyExchangeRates == null)
            {
                return NotFound();
            }

            var currencyExchangeRate = await _context.CurrencyExchangeRates
                .FirstOrDefaultAsync(m => m.CurrencyId == id);
            if (currencyExchangeRate == null)
            {
                return NotFound();
            }

            return View(currencyExchangeRate);
        }

        // POST: CurrencyExchangeRates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CurrencyExchangeRates == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.CurrencyExchangeRates'  is null.");
            }
            var currencyExchangeRate = await _context.CurrencyExchangeRates.FindAsync(id);
            if (currencyExchangeRate != null)
            {
                _context.CurrencyExchangeRates.Remove(currencyExchangeRate);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CurrencyExchangeRateExists(int id)
        {
          return _context.CurrencyExchangeRates.Any(e => e.CurrencyId == id);
        }
    }
}
