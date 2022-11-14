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

namespace WebApp.Controllers
{
    public class ProductFilesController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public ProductFilesController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: ProductFiles
        public async Task<IActionResult> Index()
        {
              return View(await _context.ProductFiles.ToListAsync());
        }

        // GET: ProductFiles/Details/5
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
                        var procesPath = Environment.ProcessPath.Replace('\\', '/');
                        var length = procesPath.LastIndexOf('/');
                        var path = Environment.ProcessPath.Remove(length);
                        path = Path.Combine(path, "Uploads");

                        if (productFile.Name.StartsWith("ICON") || productFile.Name.StartsWith("IMAGE"))
                        {
                            string type = productFile.Name.StartsWith("ICON") ? "icon" : "image";
                            string description = type == "icon" ? "Product Icon" : "Product Image"; 
                            try
                            {
                                ProductFile changedFile = SaveAndResizeFileAsync(newFile.FileName, Path.GetExtension(newFile.FileName), type, productFile, newFile, description);

                                if (changedFile != null)
                                {
                                    productFile = changedFile;  
                                    _context.ProductFiles.Update(productFile);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    ViewData["ErrorMessage"] = description + " nie został dodany";

                                    return View(productFile);
                                }

                            }
                            catch (Exception e)
                            {
                                ViewData["ErrorMessage"] = e.Message;

                                return View(productFile);

                            }

                        }
                        else
                        {

                            try
                            {
                                ProductFile changedFile = SaveAndResizeFileAsync(newFile.FileName, Path.GetExtension(newFile.FileName), "other", productFile, newFile, productFile.Description);
                                if (changedFile != null)
                                {
                                    productFile = changedFile;
                                    _context.ProductFiles.Update(productFile);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    ViewData["ErrorMessage"] = "Ikona nie została dodana";

                                    return View(productFile);
                                }

                            }
                            catch (Exception e)
                            {
                                ViewData["ErrorMessage"] = e.Message;

                                return View(productFile);

                            }




                            /*Product productFile = await _context.Products.FindAsync(productFile.ProductId);
                            *//* Using relative path of Project - files saved in WebApp/bin/Uploads*//*
                            string fileName = productFile.ProductId + "_" + Path.GetFileName(newFile.FileName);
                            *//*string path = Path.Combine(".\\bin", "Uploads");*//*
                            string extension = Path.GetExtension(newFile.FileName);
                            path = Path.Combine(path, extension.Remove(0,1));


                            if (!Directory.Exists(path))    *//* Create dir if do not exists*//*
                            {
                                Directory.CreateDirectory(path);
                            }


                            using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                *//* Save file to directory based on it's extension*//*
                                Regex regex = new Regex(@"\\Uploads\\.*");
                                var result = regex.Match(path).Captures.FirstOrDefault();
                                ProductFile file = new ProductFile(productFile.ProductId, result + "\\" + fileName, fileName, extension, productFile.Description);

                                *//* Save path, name and extension to database - remove the old one*//*
                                _context.ProductFiles.Remove(productFile);
                                _context.ProductFiles.Add(file);
                                await _context.SaveChangesAsync();


                                *//* Delete old file*//*
                                System.IO.File.Delete(path + "\\" + productFile.Name);


                                *//* Add new file to folder*//*
                                newFile.CopyTo(stream);
                            }
*/

                            TempData["FileChanged"] = "File has been changed!";

                        }
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


                return RedirectToAction("Edit", "Products", new { id = productFile.ProductId });
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


        private ProductFile SaveAndResizeFileAsync(string fileName, string extension, string fileType, ProductFile productFile, IFormFile file, string description)
        {
            var procesPath = Environment.ProcessPath.Replace('\\', '/');
            var length = procesPath.LastIndexOf('/');
            var path = Environment.ProcessPath.Remove(length);

            path = Path.Combine(path, "Uploads");
            Regex regex;// = new Regex(@"\\Uploads\\.*");
            //var result = regex.Match(path).Captures.FirstOrDefault();



            switch (fileType.ToLower())
            {
                case "icon":
                    path = Path.Combine(path, "icon");
                    regex = new Regex(@"\\Uploads\\.*");

                    if (!ValidateExtension(extension))  // Validate image extension
                    {
                        throw new Exception("Nieobsługiwany format wejściowy, obsługiwane formaty ikon: .png, .jpg, .gif, .jpeg");
                    }

                    string newIcFileName = productFile.ProductId + "_" + productFile.Name + extension;
                    string newIconName = productFile.Name;
                    var iconPath = regex.Match(path).Captures.FirstOrDefault();
                    var diskIcoPath = Path.Combine(iconPath.ToString(), newIconName);

                    //ProductFile iconFile = new ProductFile(productFile.ProductId, diskIcoPath, newIconName, extension, description);
                    productFile.Path = diskIcoPath;
                    productFile.Name = newIconName;
                    productFile.Extension = extension;   
    
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    System.IO.File.Delete(Path.Combine(path, newIconName));


                    using (var stream = new FileStream(Path.Combine(path, newIcFileName), FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    Image_resize(Path.Combine(path, newIcFileName), Path.Combine(path, newIconName), 128);

                    var IcotoDelete = Path.Combine(path.ToString(), newIcFileName);
                    System.IO.File.Delete(IcotoDelete);


                    return productFile;



                case "image":
                    path = Path.Combine(path, "image");
                    regex = new Regex(@"\\Uploads\\.*");

                    if (!ValidateExtension(extension))  // Validate image extension
                    {
                        throw new Exception("Nieobsługiwany format wejściowy, obsługiwane formaty ikon: .png, .jpg, .gif, .jpeg");
                    }

                    string newImgFileName = productFile.ProductId + "_" + productFile.Name + extension;
                    string newImageName = productFile.Name;
                    var imagePath = regex.Match(path).Captures.FirstOrDefault();
                    var diskImgPath = Path.Combine(imagePath.ToString(), newImageName);

                    //ProductFile imageFile = new ProductFile(productFile.ProductId, diskImgPath, newImageName, extension, description);
                    productFile.Path = diskImgPath;
                    productFile.Name = newImageName;
                    productFile.Extension = extension;


                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    System.IO.File.Delete(Path.Combine(path, newImageName));

                    using (var stream = new FileStream(Path.Combine(path, newImgFileName), FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    Image_resize(Path.Combine(path, newImgFileName), Path.Combine(path, newImageName), 1600);

                    var ImgtoDelete = Path.Combine(path.ToString(), newImgFileName);
                    System.IO.File.Delete(ImgtoDelete);

                    return productFile;



                case "other":
                    path = Path.Combine(path, extension.Remove(0, 1));
                    regex = new Regex(@"\\Uploads\\.*");

                    string newFileName = productFile.ProductId + "_" + fileName;
                    var filePath = regex.Match(path).Captures.FirstOrDefault();
                    var diskFilePath = Path.Combine(filePath.ToString(), newFileName);


                    //ProductFile otherFile = new ProductFile(productFile.ProductId, diskFilePath, newFileName, extension, description);
                    productFile.Path = diskFilePath;
                    productFile.Name = newFileName;
                    productFile.Extension = extension;
                    productFile.Description = description;


                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);


                    using (var stream = new FileStream(Path.Combine(path, newFileName), FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return productFile;


                default:
                    return null;
            }

        }





        /* TO SET UP ICON SIZE CHANGE new_Width and new_Height properties*/
        private void Image_resize(string input_Image_Path, string output_Image_Path, int new_Width)
        {
            SKBitmap srcBitmap = SKBitmap.Decode(input_Image_Path);

            if (srcBitmap == null)
            {
                input_Image_Path.Replace('\\', '/');
                srcBitmap = SKBitmap.Decode(input_Image_Path);
            }

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
