using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using WebApp.Context;
using WebApp.Models;
using static System.Net.Mime.MediaTypeNames;
using WebApp.Services;
using Newtonsoft.Json;
using Elastic.Clients.Elasticsearch;
using static System.Collections.Specialized.BitVector32;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [Authorize]
    public class ProductFilesController : Controller
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly AuctionFilesService _addFilesService;
        private readonly AuctionEditHistoryService editHistoryService;

        public ProductFilesController(PortalAukcyjnyContext context, 
            AuctionFilesService auctionFilesService, 
            AuctionEditHistoryService editHistoryService)
        {
            _context = context;
            this._addFilesService = auctionFilesService;
            this.editHistoryService = editHistoryService;
        }

        // GET: ProductFiles
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Index()
        {
              return View(await _context.ProductFiles.ToListAsync());
        }

        // GET: ProductFiles/Details/5
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ProductFiles == null)
            {
                return NotFound();
            }

            var productFile = await _context.ProductFiles
                .FirstOrDefaultAsync(m => m.ProductFileId == id);
            if (productFile == null)
            {
                return NotFound();
            }

            return View(productFile);
        }

        [Authorize("RequireAdmin")]
        // GET: ProductFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductFiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Create([Bind("ProductFileId,ProductId,Path,Name,Extension")] ProductFile productFile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productFile);
        }

        // GET: ProductFiles/Edit/5
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ProductFiles == null)
            {
                return NotFound();
            }

            var productFile = await _context.ProductFiles.FindAsync(id);
            if (productFile == null)
            {
                return NotFound();
            }
            return View(productFile);
        }

        // POST: ProductFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [Bind("ProductFileId,ProductId,Path,Name,Extension,Description")] ProductFile productFile, IFormFile newFile)
        {
            ModelState["newFile"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;

            if (id != productFile.ProductFileId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (newFile != null)
                    {
                        var auction = _context.Auctions.Single(x => x.AuctionId == productFile.ProductId);

                        if (productFile.Name.StartsWith("ICON") || productFile.Name.StartsWith("IMAGE"))
                        {

                            if (productFile.Name.StartsWith("ICON"))
                            {
                                var result = await _addFilesService.AddIcon(newFile, auction);
                                if (result != null) // new file has been added to DB
                                {   
                                    TempData["FileChanged"] = "File has been changed!";
                                    _context.Remove(productFile);   // deleting old file data from DB

                                }
                                else
                                TempData["FileChanged"] = "File has NOT been changed, try again!";

                                await editHistoryService.AddChangesToHistory(auction, JsonConvert.SerializeObject(result));

                            }
                            else
                            {
                                var result = _addFilesService.AddImage(newFile, auction);
                                if (result != null)
                                {
                                    TempData["FileChanged"] = "File has been changed!";
                                    _context.Remove(productFile);   // deleting old file data from DB

                                }
                                else
                                    TempData["FileChanged"] = "File has NOT been changed, try again!";

                                await editHistoryService.AddChangesToHistory(auction, JsonConvert.SerializeObject(result));

                            }

                        }
                        else
                        {
                            var result = _addFilesService.AddOrdinaryFile(newFile, auction, productFile.Description);

                            if (result != null)
                            {
                                TempData["FileChanged"] = "File has been changed!";
                                _context.Remove(productFile);   // deleting old file data from DB

                            }
                            else
                                TempData["FileChanged"] = "File has NOT been changed, try again!";

                            await editHistoryService.AddChangesToHistory(auction, JsonConvert.SerializeObject(result));

                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        TempData["FileChanged"] = "File data has been changed!";

                        _context.Update(productFile);
                        await _context.SaveChangesAsync();
                    }
                    

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductFileExists(productFile.ProductFileId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }


                return RedirectToAction("Edit", "Auctions", new { id = productFile.ProductId });
            }
            return View(productFile);
        }

        // GET: ProductFiles/Delete/5

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ProductFiles == null)
            {
                return NotFound();
            }

            var productFile = await _context.ProductFiles
                .FirstOrDefaultAsync(m => m.ProductFileId == id);
            if (productFile == null)
            {
                return NotFound();
            }

            return View(productFile);
        }

        // POST: ProductFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ProductFiles == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.ProductFiles'  is null.");
            }
            var productFile = await _context.ProductFiles.FindAsync(id);
            if (productFile != null)
            {
                _context.ProductFiles.Remove(productFile);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductFileExists(int id)
        {
          return _context.ProductFiles.Any(e => e.ProductFileId == id);
        }
    }
}
