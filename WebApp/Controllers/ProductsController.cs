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
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,Name,VatRate,IsVatExclueded")] Product product, List<IFormFile> postedFiles, IFormFile productIcon, IFormFile productImage)
        {
            /* Delete Invalid model state*/
            ModelState["Category"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productIcon"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productImage"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;


            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                /* Saving posted file in wwwroot/Uploads/ and folder based on file extension*/
                if (postedFiles.Count > 0)
                {
                    AddProductFiles(product, postedFiles);
                }
                
                /* Saving product Icon*/
                if(productIcon != null)
                {
                    AddProductIcon(product, productIcon);
                }

                /* Saving product Image*/
                if (productImage != null)
                {
                    AddProductImage(product, productImage);
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        private async Task<IActionResult> AddProductFiles(Product product, List<IFormFile> postedFiles)
        {
            /*List<string> uploadedFiles = new List<string>();*/
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
                using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    /* Save file to directory based on it's extension*/
                    ProductFile file = new ProductFile(product.ProductId, path + "\\" + fileName, fileName, extension);

                    /* Save path, name and extension to database*/
                    _context.ProductFiles.Add(file);
                    await _context.SaveChangesAsync();
                    postedFile.CopyTo(stream);
                    /*uploadedFiles.Add(fileName);*/
                }
            }
            return null;
        }

        private async Task<IActionResult> AddProductIcon(Product product, IFormFile productIcon)
        {
            string path = ".\\wwwroot\\Uploads\\icon";
            string extension = Path.GetExtension(productIcon.FileName);
            string fileName = product.ProductId + "_" + product.Name + extension;

            if (!ValidateExtension(extension))  // Validate image extension
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                /* Language change!!!*/
                ViewData["ErrorMessage"] = "Nieobsługiwany format wejściowy, obsługiwane formaty ikon: .png, .jpg, .gif, .jpeg";
                throw new Exception();
            }

            /* Create dir if do not exists*/
            if (!Directory.Exists(path))
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
            ProductFile file = new ProductFile(product.ProductId, path + "\\ICON_" + fileName.Replace(extension, ".jpg"), "ICON_" + fileName.Replace(extension, ".jpg"), ".jpg");
            _context.ProductFiles.Add(file);
            await _context.SaveChangesAsync();

            /* Delete original (NOT RESIZED) ICON file*/
            System.IO.File.Delete(path + "\\" + fileName);
            return null;

        }

        private async Task<IActionResult> AddProductImage(Product product, IFormFile productImage)
        {
            string path = ".\\wwwroot\\Uploads\\image";
            string extension = Path.GetExtension(productImage.FileName);
            string fileName = product.ProductId + "_" + product.Name + extension;

            if (!ValidateExtension(extension))  // Validate image extension
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                /* Language change!!!*/
                ViewData["ErrorMessage"] = "Nieobsługiwany format wejściowy, obsługiwane formaty ikon: .png, .jpg, .gif, .jpeg";
                throw new Exception();
            }

            /* Create dir if do not exists*/
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            /* Save original Image - it will be deleted after resizing an icon*/
            using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                await productImage.CopyToAsync(stream);
            }

            /* Resize original ICON to new Width and save as Icon_ProductId_ProductName.extension*/
            Image_resize(path + "\\" + fileName, path + "\\IMG_" + fileName.Replace(extension, ".jpg"), 1600);
            ProductFile file = new ProductFile(product.ProductId, path + "\\IMG_" + fileName.Replace(extension, ".jpg"), "IMG_" + fileName.Replace(extension, ".jpg"), ".jpg");
            _context.ProductFiles.Add(file);
            await _context.SaveChangesAsync();

            /* Delete original (NOT RESIZED) ICON file*/
            System.IO.File.Delete(path + "\\" + fileName);
            return null;

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
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CategoryId,Name,VatRate,IsVatExclueded")] Product product)
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


        /* Downloads the file (fileName) from pointed path (path)*/
        public ActionResult Download(string path, string fileName)
        {
            /* If file not found throw an exception witch message - No file found*/
            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
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
            //---------------< Image_resize() >---------------
            const long quality = 50L;
            Bitmap source_Bitmap = new Bitmap(input_Image_Path);

            double dblWidth_origial = source_Bitmap.Width;
            double dblHeigth_origial = source_Bitmap.Height;
            double relation_heigth_width = dblHeigth_origial / dblWidth_origial;
            int new_Height = (int)(new_Width * relation_heigth_width);
            //int new_Height = 70;

            //< create Empty Drawarea >
            var new_DrawArea = new Bitmap(new_Width, new_Height);
            //</ create Empty Drawarea >

            using (var graphic_of_DrawArea = Graphics.FromImage(new_DrawArea))
            {
                //< setup >
                graphic_of_DrawArea.CompositingQuality = CompositingQuality.HighSpeed;
                graphic_of_DrawArea.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic_of_DrawArea.CompositingMode = CompositingMode.SourceCopy;
                //</ setup >

                //< draw into placeholder >
                //*imports the image into the drawarea
                graphic_of_DrawArea.DrawImage(source_Bitmap, 0, 0, new_Width, new_Height);
                //</ draw into placeholder >

                //--< Output as .Jpg >--
                using (var output = System.IO.File.Open(output_Image_Path, FileMode.Create))
                {
                    //< setup jpg >
                    var qualityParamId = Encoder.Quality;
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);
                    //</ setup jpg >

                    //< save Bitmap as Jpg >
                    var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    new_DrawArea.Save(output, codec, encoderParameters);
                    
                    //resized_Bitmap.Dispose();
                    output.Close();
                    //</ save Bitmap as Jpg >
                }
                //--</ Output as .Jpg >--
                graphic_of_DrawArea.Dispose();
            }
            source_Bitmap.Dispose();
            //---------------</ Image_resize() >---------------
        }


    }

}
