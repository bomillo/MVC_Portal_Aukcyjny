using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly BreadcrumbService breadcrumbService;

        public CategoriesController(PortalAukcyjnyContext context, BreadcrumbService breadcrumbService)
        {
            _context = context;
            this.breadcrumbService = breadcrumbService;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.Categories.Include(c => c.ParentCategory);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,ParentCategoryId,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", category.ParentCategoryId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", category.ParentCategoryId);
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,ParentCategoryId,Name")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
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
            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", category.ParentCategoryId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return _context.Categories.Any(e => e.CategoryId == id);
        }


        //Displays auctions which belongs to the category with given id and it's childs 
        [Route("/Category/Auctions/{id}")]
        public async Task<IActionResult> CategoryAuctions(int? id, int page = 1)
        {
            if(id == null || _context == null)
            {
                return NotFound();
            }


            var categoriesTask = GetChildCategories(id);
            List<int> products = new List<int>();
            var mainCategory = _context.Categories.FirstOrDefault(c => c.CategoryId == id);

            ViewBag.Breadcrumb = breadcrumbService.CreateCurrentPath(mainCategory);
            var categories = await categoriesTask;
            if(categories != null)
            {
                products = _context.Products.Where(p => categories.Contains(p.CategoryId)).Select(p => p.ProductId).ToList();
            }

            var auctions = _context.Auctions.Where(a => products.Contains(a.ProductId) && a.EndTime > DateTime.UtcNow && a.IsDraft == false).Include(a => a.Product).Include(a => a.Product.Category).ToList();

            var displayAuctions = new List<DisplayAuctionsModel>();
            
            foreach(var auction in auctions)
            {
                var currentBids = _context.Bid.Where(x => x.AuctionId == auction.AuctionId).ToList();
                displayAuctions.Add(new DisplayAuctionsModel()
                {
                    Auction = auction,
                    Bid = currentBids.Select(x => x.Price).DefaultIfEmpty(0).Max()
                });
                
            }
            if (page < 1)
                page = 1;


            int rowsCount = displayAuctions.Count();
            const int pageSize = 5;
            var pager = new Pager(rowsCount, page, "Categories", pageSize, "CategoryAuctions", mainCategory.CategoryId);
            var rowsSkipped = (page - 1) * pageSize;

            var itemsPage = displayAuctions.Skip(rowsSkipped).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(itemsPage);
            
        }
        
        private async Task<List<int>> GetChildCategories(int? categoryId)
        {
            if(categoryId == null)
            {
                return null;
            }

            var newCategories = _context.Categories.Where(c => c.ParentCategoryId == categoryId).Select(c => c.CategoryId).ToList();

            List<int> categoriesId = new List<int>() { (int)categoryId };

            while (newCategories.Any())
            {
                newCategories.AddRange(_context.Categories.Where(c => c.ParentCategoryId == newCategories[0]).Select(c => c.CategoryId).ToList());

                categoriesId.Add(newCategories[0]);

                newCategories.RemoveAt(0);
            }

            return categoriesId;
        }

    }
}
