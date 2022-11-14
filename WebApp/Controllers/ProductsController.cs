using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortalAukcyjny.Models;
using WebApp.Context;
using WebApp.Models;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using System.Drawing.Drawing2D;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using SkiaSharp;

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
        public async Task<IActionResult> Index(int page = 1)
        {
            var portalAukcyjnyContext = _context.Products.Include(p => p.Category).OrderBy(p => p.ProductId);

            const int pageSize = 20;
            if (page < 1)
                page = 1;


            int rowsCount = portalAukcyjnyContext.Count();

            var pager = new Pager(rowsCount, page, "Products", pageSize);
            var rowsSkipped = (page - 1) * pageSize;

            var auctions = portalAukcyjnyContext.Skip(rowsSkipped).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            //return View(await portalAukcyjnyContext.ToListAsync());   // for displaying all auctions

            return View(auctions);  // for displaying paged auctions

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
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,Name,VatRate,IsVatExcluded")] Product product, IFormCollection postedFiles, IFormFile productIcon, IFormFile productImage)
        {

            string[] descriptions = postedFiles["fileDescription"].ToString().Split(',');

            /* Delete Invalid model state*/
            ModelState["Category"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productIcon"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productImage"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;


            if (ModelState.IsValid)
            {
                /* Used to skip icon and image file included in postedFiles*/
                int filesToSkip = 0;
                if (productIcon != null)
                    filesToSkip++;
                if (productImage != null)
                    filesToSkip++;

                int filled = descriptions.Count();
                int unfilled = postedFiles["fileDescription"].Where(s => s.Equals("")).Count();
                if (filled - unfilled < postedFiles.Files.Count - filesToSkip)
                {
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                    /* Language change!!!*/
                    ViewData["ErrorMessage"] = "Każdy załączany plik dodatkowy musi posiadać opis!";
                    return View(product);
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                
                /* Saving product Icon*/
                {
                    if (productIcon == null && productImage != null)
                        productIcon = productImage;

                    var procesPath = Environment.ProcessPath.Replace('\\', '/');
                    var length = procesPath.LastIndexOf('/');
                    var path = Environment.ProcessPath.Remove(length);
                    path = Path.Combine(path, "Uploads");
                    path = Path.Combine(path, "icon");
                    Directory.CreateDirectory(path);

                    
                    string extension;
                    string fileName;
                    if (productIcon == null)    // if no icon inserted, set default ICON_NoIcon.jpg
                    {
                        fileName = "NoIcon.jpg";
                        extension = ".jpg";
                        Regex regex = new Regex(@"\\Uploads\\.*");
                        var result = regex.Match(path).Captures.FirstOrDefault();
                        ProductFile iconFile = new ProductFile(product.ProductId, result + "\\ICON_" + fileName, "ICON_" + fileName, extension, "Product Icon");

                        _context.ProductFiles.Add(iconFile);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        extension = Path.GetExtension(productIcon.FileName);
                        fileName = product.ProductId + "_" + product.Name + extension;


                        if (!ValidateExtension(extension))  // Validate image extension
                        {
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                            /* Language change!!!*/
                            ViewData["ErrorMessage"] = "Nieobsługiwany format wejściowy, obsługiwane formaty ikon: .png, .jpg, .gif, .jpeg";
                            _context.Remove(product);
                            await _context.SaveChangesAsync();
                            throw new Exception();
                        }


                        if (!Directory.Exists(path))    /* Create dir if do not exists*/
                        {
                            Directory.CreateDirectory(path);
                        }


                        /* Save original Image - it will be deleted after resizing an icon*/
                        using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await productIcon.CopyToAsync(stream);
                        }


                        /* Resize original ICON to new Width and save as Icon_ProductId_ProductName.extension*/
                        Image_resize(path + "\\" + fileName, path + "\\ICON_" + fileName.Replace(extension, ".jpg"), 128);

                        Regex regex = new Regex(@"\\Uploads\\.*");
                        var result = regex.Match(path).Captures.FirstOrDefault();
                        ProductFile file = new ProductFile(product.ProductId, result + "\\ICON_" + fileName.Replace(extension, ".jpg"), "ICON_" + fileName.Replace(extension, ".jpg"), ".jpg", "Product Icon");


                        _context.ProductFiles.Add(file);
                        await _context.SaveChangesAsync();


                        /* Delete original (NOT RESIZED) ICON file*/
                        System.IO.File.Delete(path + "\\" + fileName);

                    }
                }


                /* Saving product Image*/
                {
                    var procesPath = Environment.ProcessPath.Replace('\\', '/');
                    var length = procesPath.LastIndexOf('/');
                    var path = Environment.ProcessPath.Remove(length);
                    path = Path.Combine(path, "Uploads");
                    path = Path.Combine(path, "image");
                    Directory.CreateDirectory(path);

                    string extension;
                    string fileName; 


                    if (productImage == null)   // if no image inserted, set default IMAGE_NoImage.jpg
                    {
                        fileName = "NoImage.jpg";
                        extension = ".jpg";

                        Regex regex = new Regex(@"\\Uploads\\.*");
                        var result = regex.Match(path).Captures.FirstOrDefault();
                        ProductFile imageFile = new ProductFile(product.ProductId, result + "\\IMAGE_" + fileName, "IMAGE_" + fileName, extension, "Product Image");

                        _context.ProductFiles.Add(imageFile);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        extension = Path.GetExtension(productIcon.FileName);
                        fileName = product.ProductId + "_" + product.Name + extension;

                        if (!ValidateExtension(extension))  // Validate image extension
                        {
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                            /* Language change!!!*/
                            ViewData["ErrorMessage"] = "Nieobsługiwany format wejściowy, obsługiwane formaty obrazów: .png, .jpg, .gif, .jpeg";

                            _context.Remove(product);
                            await _context.SaveChangesAsync();

                            throw new Exception();
                        }

                        if (!Directory.Exists(path))    /* Create dir if do not exists*/
                        {
                            Directory.CreateDirectory(path);
                        }

                        /* Save original Image - it will be deleted after resizing an icon*/
                        using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await productImage.CopyToAsync(stream);
                        }

                        /* Resize original ICON to new Width and save as Icon_ProductId_ProductName.extension*/
                        Image_resize(path + "\\" + fileName, path + "\\IMAGE_" + fileName.Replace(extension, ".jpg"), 1600);
                        
                        
                        Regex regex = new Regex(@"\\Uploads\\.*");
                        var result = regex.Match(path).Captures.FirstOrDefault();
                        ProductFile file = new ProductFile(product.ProductId, result + "\\IMAGE_" + fileName.Replace(extension, ".jpg"), "IMAGE_" + fileName.Replace(extension, ".jpg"), ".jpg", "Product Icon");

                        _context.ProductFiles.Add(file);
                        await _context.SaveChangesAsync();

                        /* Delete original (NOT RESIZED) ICON file*/
                        System.IO.File.Delete(path + "\\" + fileName);
                    }


                }


                /* Saving posted file in bin/Uploads/ and folder based on file extension*/
                if (postedFiles.Count > 0)
                {
                    int iterator = 0;
                    foreach (IFormFile postedFile in postedFiles.Files)
                    {
                        if(filesToSkip > 0)
                        {
                            filesToSkip--;
                            continue;
                        }

                        string fileName = product.ProductId + "_" + Path.GetFileName(postedFile.FileName);

                        var procesPath = Environment.ProcessPath.Replace('\\', '/');
                        string extension = Path.GetExtension(postedFile.FileName);
                        var length = procesPath.LastIndexOf('/');
                        var path = Environment.ProcessPath.Remove(length);
                        
                        path = Path.Combine(path, "Uploads");
                        path = Path.Combine(path, extension.Remove(0,1));

                        /*string path = Path.Combine(".\\bin", "Uploads");    *//* Using relative path of Project - files saved in WebApp/bin/Uploads*/
                        //path += extension.Replace('.', '\\');

                        Directory.CreateDirectory(path);

                        using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            /* Save file to directory based on it's extension*/
                            Regex regex = new Regex(@"\\Uploads\\.*");
                            var result = regex.Match(path).Captures.FirstOrDefault();
                            ProductFile file = new ProductFile(product.ProductId, result + "\\" + fileName, fileName, extension, descriptions?[iterator++]);

                            /* Save path, name and extension to database*/
                            _context.ProductFiles.Add(file);
                            await _context.SaveChangesAsync();
                            postedFile.CopyTo(stream);
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
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CategoryId,Name,VatRate,IsVatExcluded")] Product product, IFormCollection postedFiles, IFormFile productIcon, IFormFile productImage)
        {
            ModelState["Category"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productIcon"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productImage"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;

            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string[] descriptions = postedFiles["fileDescription"].ToString().Split(',');

                try
                {
                    /**/
                    int filesToSkip = 0;
                    if (productIcon != null)
                        filesToSkip++;
                    if (productImage != null)
                        filesToSkip++;

                    int filled = descriptions.Count();
                    int unfilled = postedFiles["fileDescription"].Where(s => s.Equals("")).Count();
                    if (filled - unfilled < postedFiles.Files.Count - filesToSkip)
                    {
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        /* Language change!!!*/
                        ViewData["ErrorMessage"] = "Każdy załączany plik dodatkowy musi posiadać opis!";
                        return View(product);
                    }

                    /* Saving product Icon*/
                    if(productIcon != null || productImage != null)
                    {
                        if (productIcon == null && productImage != null)
                            productIcon = productImage;

                        var procesPath = Environment.ProcessPath.Replace('\\', '/');
                        var length = procesPath.LastIndexOf('/');
                        var path = Environment.ProcessPath.Remove(length);

                        path = Path.Combine(path, "Uploads");
                        path = Path.Combine(path, "icon");

                        string extension;
                        string fileName;
                        if (productIcon == null)    // if no icon inserted, set default ICON_NoIcon.jpg
                        {
                            fileName = "NoIcon.jpg";
                            extension = ".jpg";
                            Regex regex = new Regex(@"\\Uploads\\.*");
                            var result = regex.Match(path).Captures.FirstOrDefault();
                            ProductFile iconFile = new ProductFile(product.ProductId, result + "\\ICON_" + fileName, "ICON_" + fileName, extension, "Product Icon");

                            _context.ProductFiles.Add(iconFile);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            extension = Path.GetExtension(productIcon.FileName);
                            fileName = product.ProductId + "_" + product.Name + extension;


                            if (!ValidateExtension(extension))  // Validate image extension
                            {
                                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                                /* Language change!!!*/
                                ViewData["ErrorMessage"] = "Nieobsługiwany format wejściowy, obsługiwane formaty ikon: .png, .jpg, .gif, .jpeg";
                                await _context.SaveChangesAsync();
                                throw new Exception();
                            }


                            if (!Directory.Exists(path))    /* Create dir if do not exists*/
                                Directory.CreateDirectory(path);


                            /* Save original Image - it will be deleted after resizing an icon*/
                            using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await productIcon.CopyToAsync(stream);
                            }


                            /* Resize original ICON to new Width and save as Icon_ProductId_ProductName.extension*/
                            Image_resize(path + "\\" + fileName, path + "\\ICON_" + fileName.Replace(extension, ".jpg"), 128);

                            Regex regex = new Regex(@"\\Uploads\\.*");
                            var result = regex.Match(path).Captures.FirstOrDefault(); 
                            ProductFile file = new ProductFile(product.ProductId, result + "\\ICON_" + fileName.Replace(extension, ".jpg"), "ICON_" + fileName.Replace(extension, ".jpg"), ".jpg", "Product Icon");


                            _context.ProductFiles.Add(file);
                            await _context.SaveChangesAsync();


                            /* Delete original (NOT RESIZED) ICON file*/
                            System.IO.File.Delete(path + "\\" + fileName);

                        }
                    }


                    /* Saving product Image*/
                    if(productImage != null)
                    {
                        var procesPath = Environment.ProcessPath.Replace('\\', '/');
                        var length = procesPath.LastIndexOf('/');
                        var path = Environment.ProcessPath.Remove(length);
                        path = Path.Combine(path, "Uploads");
                        path = Path.Combine(path, "image");
                        Directory.CreateDirectory(path);

                        string extension;
                        string fileName;


                        if (productImage == null)   // if no image inserted, set default IMAGE_NoImage.jpg
                        {
                            fileName = "NoImage.jpg";
                            extension = ".jpg";
                            Regex regex = new Regex(@"\\Uploads\\.*");
                            var result = regex.Match(path).Captures.FirstOrDefault();
                            ProductFile imageFile = new ProductFile(product.ProductId, result + "\\IMAGE_" + fileName, "IMAGE_" + fileName, extension, "Product Image");

                            _context.ProductFiles.Add(imageFile);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            extension = Path.GetExtension(productIcon.FileName);
                            fileName = product.ProductId + "_" + product.Name + extension;

                            if (!ValidateExtension(extension))  // Validate image extension
                            {
                                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                                /* Language change!!!*/
                                ViewData["ErrorMessage"] = "Nieobsługiwany format wejściowy, obsługiwane formaty obrazów: .png, .jpg, .gif, .jpeg";

                                _context.Remove(product);
                                await _context.SaveChangesAsync();

                                throw new Exception();
                            }

                            if (!Directory.Exists(path))    /* Create dir if do not exists*/
                            {
                                Directory.CreateDirectory(path);
                            }

                            /* Save original Image - it will be deleted after resizing an icon*/
                            using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await productImage.CopyToAsync(stream);
                            }

                            /* Resize original ICON to new Width and save as Icon_ProductId_ProductName.extension*/
                            Image_resize(path + "\\" + fileName, path + "\\IMAGE_" + fileName.Replace(extension, ".jpg"), 1600);
                            Regex regex = new Regex(@"\\Uploads\\.*");
                            var result = regex.Match(path).Captures.FirstOrDefault();
                            ProductFile file = new ProductFile(product.ProductId, result + "\\IMAGE_" + fileName.Replace(extension, ".jpg"), "IMAGE_" + fileName.Replace(extension, ".jpg"), ".jpg", "Product Image");

                            _context.ProductFiles.Add(file);
                            await _context.SaveChangesAsync();

                            /* Delete original (NOT RESIZED) ICON file*/
                            System.IO.File.Delete(path + "\\" + fileName);
                        }


                    }


                    /* Saving posted file in bin/Uploads/ and folder based on file extension*/
                    if (postedFiles.Count > 0)
                    {
                        int iterator = 0;
                        foreach (IFormFile postedFile in postedFiles.Files)
                        {
                            if (filesToSkip-- > 0)
                                continue;

                            string fileName = product.ProductId + "_" + Path.GetFileName(postedFile.FileName);

                            var procesPath = Environment.ProcessPath.Replace('\\', '/');
                            string extension = Path.GetExtension(postedFile.FileName);
                            var length = procesPath.LastIndexOf('/');
                            var path = Environment.ProcessPath.Remove(length);
                            path = Path.Combine(path, "Uploads");
                            path = Path.Combine(path, extension.Remove(0,1));


                            if (!Directory.Exists(path))    /* Create dir if do not exists*/
                                Directory.CreateDirectory(path);

                            using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                /* Save file to directory based on it's extension*/
                                Regex regex = new Regex(@"\\Uploads\\.*");
                                var result = regex.Match(path).Captures.FirstOrDefault();
                                ProductFile file = new ProductFile(product.ProductId, result + "\\" + fileName, fileName, extension, descriptions?[iterator++]);

                                /* Save path, name and extension to database*/
                                _context.ProductFiles.Add(file);
                                await _context.SaveChangesAsync();
                                postedFile.CopyTo(stream);
                            }
                        }
                    }


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
            var productFiles = from file in _context.ProductFiles
                   .Where(prod => prod.ProductId == id)
                               select file;
            ViewData["ProductFiles"] = productFiles;
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


        /* Downloads the file (fileName) from pointed path (path)*/
        public ActionResult Download(string path, string fileName)
        {
            /* If file not found throw an exception witch message - No file found*/
            var procesPath = Environment.ProcessPath.Replace('\\', '/');
            var length = procesPath.LastIndexOf('/');
            var folderPath = Environment.ProcessPath.Remove(length);
            folderPath += path;
            
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(folderPath);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    var NotFoundViewModel = new ErrorViewModel { RequestId = new string("No file found") };
                    return View("Error", NotFoundViewModel);
                }
            }
            catch (FileNotFoundException e)
            {
                var NotFoundViewModel = new ErrorViewModel { RequestId = new string("No file found") };
                return View("Error", NotFoundViewModel);
            }
        }

        /* Checks if user inserted proper file*/
        private bool ValidateExtension(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".jpg":
                    return true;
                case ".png":
                    return true;
                case ".gif":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }

        /* TO SET UP ICON SIZE CHANGE new_Width and new_Height properties*/
        private void Image_resize(string input_Image_Path, string output_Image_Path, int new_Width)
        {
            SKBitmap srcBitmap = SKBitmap.Decode(input_Image_Path);
            double dblWidth_origial = srcBitmap.Width;
            double dblHeigth_origial = srcBitmap.Height;
            double relation_heigth_width = dblHeigth_origial / dblWidth_origial;
            int new_Height = (int)(new_Width * relation_heigth_width);

            SKImageInfo resizeInfo = new SKImageInfo(new_Width, new_Height);

            // SKImage resizes blurry
            using (var surface = SKSurface.Create(resizeInfo))
            using (var paint = new SKPaint())
            {
                // high quality with antialiasing
                paint.IsAntialias = true;
                paint.FilterQuality = SKFilterQuality.High;

                // draw the bitmap to fill the surface
                surface.Canvas.DrawBitmap(srcBitmap, new SKRectI(0, 0, new_Width, new_Height),
                    paint);
                surface.Canvas.Flush();

                // save
                using (var newImg = surface.Snapshot())
                using (SKData data = newImg.Encode(SKEncodedImageFormat.Jpeg, 100))
                using (var stream = new FileStream(output_Image_Path, FileMode.Create, FileAccess.Write))
                    data.SaveTo(stream);
            }

            // SKBitmap resizes crisp.
            using (SKBitmap resizedSKBitmap = srcBitmap.Resize(resizeInfo, SKBitmapResizeMethod.Lanczos3))
            using (SKImage newImg = SKImage.FromPixels(resizedSKBitmap.PeekPixels()))
            using (SKData data = newImg.Encode(SKEncodedImageFormat.Jpeg, 90))
            using (var stream = new FileStream(output_Image_Path, FileMode.Create, FileAccess.Write))
                data.SaveTo(stream);

        }
    }
}
