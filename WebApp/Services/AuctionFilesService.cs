using SkiaSharp;
using System.Text.RegularExpressions;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Services
{
    public class AuctionFilesService
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly string iconErrorPath = "/image/noIcon.png";
        private readonly string imageErrorPath = "/image/NoImage.png";

        public AuctionFilesService(PortalAukcyjnyContext context)
        {
            this._context = context;
        }

        public string GetErrorIconPath()
        {
            return iconErrorPath;
        }
        public string GetErrorImagePath()
        {
            return imageErrorPath;
        }

        public bool IconExist(Auction auction)
        {
            if(auction != null)
            {
                var icon = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("ICON_")).FirstOrDefault();

                if(icon != null) 
                {
                    return true;
                }
            }
            return false;

        }


        public bool ImageExist(Auction auction)
        {
            if (auction != null)
            {
                var image = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("IMAGE_")).FirstOrDefault();

                if (image != null)
                {
                    return true;
                }
            }
            return false;

        }


        public bool FileExist(Auction auction, string FileName) 
        {
            if (auction != null) 
            {
                var file = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name == FileName).FirstOrDefault();

                if(file != null)
                {
                    return true;
                }
            }
            return false;
        }


        public string? GetIconPath(Auction auction)
        {
            if (auction != null)
            {
                var icon = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("ICON_")).FirstOrDefault();

                if (icon != null)
                {
                    return icon.Path;
                }
            }
            return null;
        }


        public string? GetImagePath(Auction auction)
        {
            if (auction != null)
            {
                var image = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("IMAGE_")).FirstOrDefault();

                if (image != null)
                {
                    return image.Path;
                }
            }
            return null;
        }



        public ProductFile? AddIcon(IFormFile iconFile, Auction auction)
        {
            try
            {

                ProductFile createdFile = SaveAndResizeFileAsync(iconFile.FileName, ".jpg", "icon", auction, iconFile, "Icon");

                if (createdFile != null)
                {
                    _context.ProductFiles.Add(createdFile);

                    return createdFile;

                }
                else
                {
                    return null;

                }

            }
            catch (Exception e)
            {
                return null;
            }
        }



        public ProductFile? AddImage(IFormFile imageFile, Auction auction)
        {
            if (imageFile != null)
            {
                try
                {

                    ProductFile createdFile = SaveAndResizeFileAsync(imageFile.FileName, ".jpg", "image", auction, imageFile, "Image");

                    if (createdFile != null)
                    {
                        _context.ProductFiles.Add(createdFile);
                        return createdFile;
                    }
                    else
                    {
                        return null;

                    }

                }
                catch (Exception e)
                {
                    return null;

                }
            }

            return null;
        }



        public ProductFile? AddOrdinaryFile(IFormFile postedFile, Auction auction, string description)
        {
            try
            {

                ProductFile createdFile = SaveAndResizeFileAsync(postedFile.FileName, Path.GetExtension(postedFile.FileName), "other", auction, postedFile, description);

                if (createdFile != null)
                {
                    _context.ProductFiles.Add(createdFile);
                    return createdFile;
                }
                else
                {
                    return null;

                }
            }
            catch (Exception e)
            {
                return null;
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




        private ProductFile SaveAndResizeFileAsync(string fileName, string extension, string fileType, Auction auction, IFormFile file, string description)
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
                    regex = new Regex(@"[\\|\/]Uploads[\\|\/].*");


                    if (!ValidateExtension(extension))  // Validate image extension
                    {
                        throw new Exception("Nieobsługiwany format wejściowy, obsługiwane formaty ikon: .png, .jpg, .gif, .jpeg");
                    }


                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);


                    string newIcFileName = auction.AuctionId + "_" + auction.Title + extension;
                    string newIconName = ("ICON_" + auction.AuctionId + "_" + auction.Title + ".jpg").Replace(' ', '_');
                    var iconPath = regex.Match(path).Captures.FirstOrDefault();
                    var diskIcoPath = Path.Combine(iconPath.ToString(), newIconName);


                    ProductFile iconFile = new ProductFile(auction.AuctionId, diskIcoPath, newIconName, extension, description);


                    /* Copy file to disk storage*/
                    using (var stream = new FileStream(Path.Combine(path, newIcFileName), FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    /* Resize image and save it in Path.Combine(path, newIconName)*/
                    Image_resize(Path.Combine(path, newIcFileName), Path.Combine(path, newIconName), 128);


                    var IcotoDelete = Path.Combine(path.ToString(), newIcFileName);
                    /* Delete model file*/
                    System.IO.File.Delete(IcotoDelete);


                    return iconFile;



                case "image":
                    path = Path.Combine(path, "image");
                    regex = new Regex(@"[\\|\/]Uploads[\\|\/].*");



                    if (!ValidateExtension(extension))  // Validate image extension
                    {
                        throw new Exception("Nieobsługiwany format wejściowy, obsługiwane formaty ikon: .png, .jpg, .gif, .jpeg");
                    }


                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);


                    string newImgFileName = auction.AuctionId + "_" + auction.Title + extension;
                    string newImageName = ("IMAGE_" + auction.AuctionId + "_" + auction.Title + ".jpg").Replace(' ', '_');
                    var imagePath = regex.Match(path).Captures.FirstOrDefault();
                    var diskImgPath = Path.Combine(imagePath.ToString(), newImageName);



                    ProductFile imageFile = new ProductFile(auction.AuctionId, diskImgPath, newImageName, extension, description);


                    using (var stream = new FileStream(Path.Combine(path, newImgFileName), FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }


                    Image_resize(Path.Combine(path, newImgFileName), Path.Combine(path, newImageName), 1600);


                    var ImgtoDelete = Path.Combine(path.ToString(), newImgFileName);
                    System.IO.File.Delete(ImgtoDelete);


                    return imageFile;



                case "other":
                    path = Path.Combine(path, extension.Remove(0, 1));
                    regex = new Regex(@"[\\|\/]Uploads[\\|\/].*");

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);


                    string newFileName = auction.AuctionId + "_" + fileName;
                    var filePath = regex.Match(path).Captures.FirstOrDefault();
                    var diskFilePath = Path.Combine(filePath.ToString(), newFileName);


                    ProductFile otherFile = new ProductFile(auction.AuctionId, diskFilePath, newFileName, extension, description);


                    using (var stream = new FileStream(Path.Combine(path, newFileName), FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return otherFile;


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
                input_Image_Path = input_Image_Path.Replace('\\', '/');
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
