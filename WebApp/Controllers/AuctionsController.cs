using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebApp.Context;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class AuctionsController : Controller
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly BreadcrumbService breadcrumbService;

        public AuctionsController(PortalAukcyjnyContext context, BreadcrumbService breadcrumbService)
        {
            _context = context;
            this.breadcrumbService = breadcrumbService;
        }

        // GET: Auctions
        public async Task<IActionResult> Index(int page = 1)
        {
            var portalAukcyjnyContext = _context.Auctions
                .Include(a => a.Owner)
                .Include(a => a.Product).OrderBy(a => a.AuctionId);

            const int pageSize = 20;
            if(page < 1)
                page = 1;

            int rowsCount = portalAukcyjnyContext.Count();

            var pager = new Pager(rowsCount, page, "Auctions", pageSize );

            var rowsSkipped = (page - 1) * pageSize;

            var auctions = portalAukcyjnyContext.Skip(rowsSkipped).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            //return View(await portalAukcyjnyContext.ToListAsync());   // for displaying all auctions

            return View(auctions);  // for displaying paged auctions

        }

        // GET: Auctions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Auctions == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions
                .Include(a => a.Owner)
                .Include(a => a.Product)
                .FirstOrDefaultAsync(m => m.AuctionId == id);
            if (auction == null)
            {
                return NotFound();
            }

            ViewBag.Breadcrumb = breadcrumbService.CreateCurrentPath(auction);

            return View(auction);
        }

        // GET: Auctions/Create
        public IActionResult Create()
        {
            //var userId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.ValueType == "userid").Value.ToString());
            var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value.ToString());

            ViewBag.OwnerId = userId;
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name");
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuctionId,Description,Title,IsDraft,OwnerId,CreationTime,PublishedTime,EndTime,ProductId")] Auction auction)
        {
            ModelState["Owner"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["Product"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            
            if(auction.IsDraft)
            {
                auction.PublishedTime = null;
                auction.EndTime = null;
            }
            else
            {
                if (auction.PublishedTime > auction.EndTime)
                {
                    /*ViewBag.DatesMsg = WebApp.Resources.Shared.;*/
                    ViewBag.DatesMsg = "Dates are not valid - end time is before publishing!";
                    ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                    ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                    return View(auction);
                }

                auction.PublishedTime = DateTime.SpecifyKind(auction.PublishedTime.Value, DateTimeKind.Utc);
                auction.EndTime = DateTime.SpecifyKind(auction.EndTime.Value, DateTimeKind.Utc);

            }

            if (ModelState.IsValid)
            {
                auction.CreationTime = DateTime.SpecifyKind(DateTime.Now.ToUniversalTime(), DateTimeKind.Utc);

                _context.Add(auction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value.ToString());
            
            ViewBag.OwnerId = userId;
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
            
            return View(auction);
        }

        // GET: Auctions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Auctions == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
            {
                return NotFound();
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
            return View(auction);
        }

        // POST: Auctions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AuctionId,Description,Title,IsDraft,OwnerId,CreationTime,PublishedTime,EndTime,ProductId")] Auction auction)
        {
            ModelState["Owner"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["Product"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;


            if (id != auction.AuctionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(!auction.IsDraft)
                    {
                        if(!(auction.PublishedTime == null || auction.EndTime == null))
                        {
                            auction.PublishedTime = DateTime.SpecifyKind(auction.PublishedTime.Value, DateTimeKind.Utc);
                            auction.EndTime = DateTime.SpecifyKind(auction.EndTime.Value, DateTimeKind.Utc);
                        }
                        else
                        {
                            ViewBag.DatesMsg = "Both dates must be submited when publishing auction!";
                            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
                            return View(auction);
                        }

                        if(auction.PublishedTime > auction.EndTime)
                        {
                            /*ViewBag.DatesMsg = WebApp.Resources.Shared.;*/
                            ViewBag.DatesMsg = "Dates are not valid - end time is before publishing!";
                            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
                            return View(auction);
                        }


                    }
                    else
                    {
                        auction.PublishedTime = null;
                        auction.EndTime = null;
                    }

                    auction.CreationTime = DateTime.SpecifyKind(auction.CreationTime, DateTimeKind.Utc);

                    _context.Auctions.Update(auction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionExists(auction.AuctionId))
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
            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
            return View(auction);
        }

        // GET: Auctions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Auctions == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions
                .Include(a => a.Owner)
                .Include(a => a.Product)
                .FirstOrDefaultAsync(m => m.AuctionId == id);
            if (auction == null)
            {
                return NotFound();
            }

            return View(auction);
        }

        // POST: Auctions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Auctions == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.Auctions'  is null.");
            }
            var auction = await _context.Auctions.FindAsync(id);
            if (auction != null)
            {
                _context.Auctions.Remove(auction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuctionExists(int id)
        {
          return _context.Auctions.Any(e => e.AuctionId == id);
        }
    }
}
