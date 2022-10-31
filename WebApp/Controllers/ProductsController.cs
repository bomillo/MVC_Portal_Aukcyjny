using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortalAukcyjny.Models;
using WebApp.Context;
using WebApp.Models;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace WebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly PortalAukcyjnyContext _context; 
        private IHostingEnvironment _environment;

        public ProductsController(PortalAukcyjnyContext context, IHostingEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.Products.Include(p => p.Category);
                                
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            /* Get all files that refers to actual product*/
            var productFiles = from file in _context.ProductFiles
                               .Where(prod => prod.ProductId == id)
                               select file;

            /* Create a list of items, to display for user*/
            List<ProductFile> items = new List<ProductFile>();
            foreach (var file in productFiles)
            {
                items.Add(file);
            }
               
            ViewBag.Items = items;

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        /* Downloads the file (fileName) from pointed path (path)*/
        public ActionResult Download(string path, string fileName)
        {
            /* If file not found throw an exception witch message - No file found*/
            try 
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch(FileNotFoundException e)
            {
                var NotFoundViewModel = new ErrorViewModel { RequestId = new string("No file found") };
                return View("Error", NotFoundViewModel);
            }
        }



        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,Name,VatRate,IsVatExcluded")] Product product, List<IFormFile> postedFiles)
        {
            /* Delete Invalid model state*/
            ModelState["Category"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                /* Saving posted file in wwwroot/Uploads/ and folder based on file extension*/
                if (postedFiles.Count > 0)
                {
                    string wwwPath = this._environment.WebRootPath;
                    string contentPath = this._environment.ContentRootPath;

                    List<string> uploadedFiles = new List<string>();
                    foreach (IFormFile postedFile in postedFiles)
                    {
                        /* Using relative path of Project - files saved in WebApp/wwwroot/Uploads*/
                        string fileName = Path.GetFileName(postedFile.FileName);
                        string path = Path.Combine(".\\wwwroot", "Uploads");
                        string extension = Path.GetExtension(postedFile.FileName);
                        path += extension.Replace('.', '\\');

                        /* Create dir if do not exists*/
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        using (FileStream stream = new FileStream(Path.Combine(path , fileName), FileMode.Create))
                        {
                            /* Save file to directory based on it's extension*/
                            ProductFile file = new ProductFile(product.ProductId, path+"\\"+fileName, fileName, extension);

                            /* Save path, name and extension to database*/
                            _context.ProductFiles.Add(file);
                            await _context.SaveChangesAsync();
                            postedFile.CopyTo(stream);
                            uploadedFiles.Add(fileName);
                        }
                    }
                }
                    return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            /* Get all files connected with product*/
            var productFiles = from file in _context.ProductFiles
                               .Where(prod => prod.ProductId == id)
                               select file;

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            ViewData["ProductFiles"] = productFiles;
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CategoryId,Name,VatRate,IsVatExcluded")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
