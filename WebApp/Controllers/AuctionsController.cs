using System;
using System.Collections.Generic;
using System.Globalization;
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
using PortalAukcyjny.Models;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class AuctionsController : Controller
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly BreadcrumbService breadcrumbService;
        private readonly AuctionFilesService _auctionFilesService;
        private readonly ObservAuctionService _observedAuctionService;
        private readonly BidsService bidsService;
        private readonly SetPagerService pagerService;

        public AuctionsController(PortalAukcyjnyContext context, 
            ObservAuctionService observAuctionService, 
            BreadcrumbService breadcrumbService, 
            AuctionFilesService auctionFileService, 
            BidsService bidsService,
            SetPagerService pagerService)
        {
            _context = context;
            this._observedAuctionService = observAuctionService;
            this.breadcrumbService = breadcrumbService;

            this.bidsService = bidsService;
            this.pagerService = pagerService;
            this._auctionFilesService = auctionFileService;

            

        }

        // GET: Auctions
        public async Task<IActionResult> Index(int page = 1)
        {
            if(page < 1)
                page = 1;
            
            var portalAukcyjnyContext = _context.Auctions
                .Include(a => a.Owner)
                .Include(a => a.Product).OrderBy(a => a.AuctionId);

            int userId = -1;
            if(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")) != null)
                int.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value, out userId);

            int pageSize = await pagerService.SetPager(userId);
            int rowsCount = portalAukcyjnyContext.Count();
            
            var pager = new Pager(rowsCount, page, "Auctions", pageSize);
            var rowsSkipped = (page - 1) * pageSize;
            var auctions = portalAukcyjnyContext.Skip(rowsSkipped).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;


            List<DisplayAuctionsModel> auctionList = new List<DisplayAuctionsModel>();

            foreach (var auction in auctions)
            {
                string path = null;
                var icon = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("ICON")).FirstOrDefault();

                if (icon != null)
                    path = icon.Path;
                else
                    path = _auctionFilesService.GetErrorIconPath();

                var Bid = _context.Bid.Where(x => x.AuctionId == auction.AuctionId).ToList();
                auctionList.Add(new DisplayAuctionsModel()
                {
                    Auction = auction,
                    iconPath = path,
                    Bid = Bid.Select(x => x.Price).DefaultIfEmpty(0).Max()
                });
            }


            return View(auctionList);  // for displaying paged auctions
            
            //return View(await portalAukcyjnyContext.ToListAsync());   // for displaying all auctions
        }

        // GET: Auctions/Details/5
        public async Task<IActionResult> Details(int? id, string? result)
        {
            if (id == null || _context.Auctions == null)
            {
                return NotFound();
            }
            
            ViewBag.result = result;

            var auction = await _context.Auctions
                .Include(a => a.Owner)
                .Include(a => a.Product)
                .FirstOrDefaultAsync(m => m.AuctionId == id);

            /* Get all files that refers to actual product*/
            var productFiles = (from file in _context.ProductFiles
                               .Where(x => x.ProductId == id)
                                select file).ToList();

            ViewBag.Items = productFiles;


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
            var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value);

            ViewBag.OwnerId = userId;
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name");
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuctionId,Description,Title,IsDraft,OwnerId,CreationTime,PublishedTime,EndTime,ProductId")] Auction auction, IFormCollection postedFiles, IFormFile productIcon, IFormFile productImage)
        {
            string[] descriptions = postedFiles["fileDescription"].ToString().Split(',');

            ModelState["Owner"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["Product"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productIcon"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productImage"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;

            if (auction.IsDraft)
            {
                auction.PublishedTime = null;
                auction.EndTime = null;
            }
            else
            {
                if ((auction.PublishedTime > auction.EndTime) || (auction.PublishedTime == null || auction.EndTime == null))
                {
                    /*ViewBag.DatesMsg = WebApp.Resources.Shared.;*/
                    ViewBag.DatesMsg = "Dates are not valid!";
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

                //var product = _context.Products.Where(p => p.AuctionId == auction.AuctionId).FirstOrDefault();
                int filesToSkip = 0;

                if (productIcon != null || productImage != null)
                {
                    if (productIcon == null && productImage != null)
                    {
                        productIcon = productImage;
                        filesToSkip--;
                    }

                    filesToSkip++;

                    string result = _auctionFilesService.AddIcon(productIcon, auction).ToString();

                    if (result == null)
                    {
                        ViewBag.ErrorMessage = "Uneable to add productIcon file, please try again!";
                        ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                        ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                        return View(auction);
                    }

                }

                if (productImage != null)
                {
                    filesToSkip++;

                    string result = _auctionFilesService.AddImage(productImage, auction).ToString();

                    if (result == null)
                    {
                        ViewBag.ErrorMessage = "Uneable to add productImage file, please try again!";
                        ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                        ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                        return View(auction);
                    }

                }

                if (postedFiles != null)
                {
                    int i = 0;
                    foreach (IFormFile file in postedFiles.Files)
                    {

                        if (filesToSkip-- > 0)
                            continue;

                        string result = _auctionFilesService.AddOrdinaryFile(file, auction, descriptions[i++]).ToString();

                        if (result == null)
                        {
                            ViewBag.ErrorMessage = "Uneable to add productImage file, please try again!";
                            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                            return View(auction);
                        }

                    }

                }

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

            /* Get all files connected with auction*/
            var productFiles = from file in _context.ProductFiles
                               .Where(prod => prod.ProductId == id)
                               select file;


            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
            {
                return NotFound();
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
            ViewData["ProductFiles"] = productFiles;
            return View(auction);
        }

        // POST: Auctions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AuctionId,Description,Title,IsDraft,OwnerId,CreationTime,PublishedTime,EndTime,ProductId")] Auction auction, IFormCollection postedFiles, IFormFile productIcon, IFormFile productImage)
        {
            string[] descriptions = postedFiles["fileDescription"].ToString().Split(',');

            ModelState["Owner"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["Product"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productIcon"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productImage"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;


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


                    int filesToSkip = 0;

                    if (productIcon != null || productImage != null)
                    {
                        if (productIcon == null && productImage != null && !_auctionFilesService.IconExist(auction))
                        {
                            productIcon = productImage;
                            filesToSkip--;
                        }

                        filesToSkip++;

                        string result = _auctionFilesService.AddIcon(productIcon, auction).ToString();

                        if (result == null)
                        {
                            ViewBag.ErrorMessage = "Uneable to add productIcon file, please try again!";
                            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                            return View(auction);
                        }

                    }

                    if (productImage != null)
                    {
                        filesToSkip++;

                        string result = _auctionFilesService.AddImage(productImage, auction).ToString();

                        if (result == null)
                        {
                            ViewBag.ErrorMessage = "Uneable to add productImage file, please try again!";
                            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                            return View(auction);
                        }

                    }

                    if (postedFiles != null)
                    {
                        int i = 0;
                        foreach (IFormFile file in postedFiles.Files)
                        {

                            if (filesToSkip-- > 0)
                                continue;

                            string result = _auctionFilesService.AddOrdinaryFile(file, auction, descriptions[i++]).ToString();

                            if (result == null)
                            {
                                ViewBag.ErrorMessage = "Uneable to add productImage file, please try again!";
                                ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                                ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                                return View(auction);
                            }

                        }

                    }
                    
                    _context.Auctions.Update(auction);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException e)
                {
                    if (!AuctionExists(auction.AuctionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        var NotFoundViewModel = new ErrorViewModel { RequestId = new string("Error occured, refresh page and try again!") };
                        return View("Error", NotFoundViewModel);
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


        // POST: Auctions/ObservAuction/5 
        public ActionResult ObservAuction(int? id)
        {
            string resultMsg = "";
            if(id > 0)
            {
                int userId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value.ToString());

                if(userId > 0)
                {
                    var result = _observedAuctionService.Observe((int)id, userId);
                    if (result != null)
                    {
                        resultMsg = "Auction is now being observed";
                    
                        return RedirectToAction("Details", new { id = id, result = resultMsg});
                    }
                    resultMsg = "Auction already observed";

                    return RedirectToAction("Details", new { id = id, result = resultMsg });
                }
            }
            resultMsg = "Invalid data, please try again";

            return RedirectToAction("Details", new { id = id, result = resultMsg });
        }

        public async Task<ActionResult> Auction(int? id)
        {
            if(id == null)
            {
                return BadRequest();
            }
            var bidsTask = bidsService.GetAuctionBids((int)id);
            var questionTask = GetAuctionQuestions((int)id);
            var auction = _context.Auctions.FirstOrDefault(x => x.AuctionId == id);
            if(auction == null)
            {
                return BadRequest();
            }

            var auctionDTO = new DisplaySingleAuctionModel()
            {
                AuctionId = auction.AuctionId,
                OwnerId = auction.OwnerId,
                Title = auction.Title,
                Description = auction.Description,
                EndDate = auction.EndTime.GetValueOrDefault().ToString(),
                TimeToEnd = $"{WebApp.Resources.Shared.EndIn} {(auction.EndTime - DateTime.UtcNow).GetValueOrDefault().Days.ToString()} {WebApp.Resources.Shared.Days}"
            };
            auction.Product = _context.Products.FirstOrDefault(x => x.ProductId == auction.ProductId);
            ViewBag.Breadcrumb = breadcrumbService.CreateCurrentPath(auction);

            auctionDTO.Questions = await questionTask;
            auctionDTO.Bids = await bidsTask;

            //todo upload photo

            auctionDTO.Images = new List<string>()
            {
                ""
            };

            return View(auctionDTO);
        }

        private async Task<List<QuestionDTO>> GetAuctionQuestions(int auctionId)
        {
           var questions = _context.AuctionQuestion.Where(x => x.AuctionId == auctionId)
                                    .Include(u => u.User)
                                    .OrderByDescending(o => o.PublishedTime)
                                    .ToList();

            var questionsDTO = new List<QuestionDTO>();

            foreach(var question in questions)
            {
                questionsDTO.Add(new QuestionDTO()
                {
                    Question = question.Question,
                    Answer = string.IsNullOrWhiteSpace(question.Answer) ? String.Empty : question.Answer,
                    QuestionId = question.QuestionId,
                    AnsweredTime = question.AnsweredTime,
                    AskedTime = question.PublishedTime,
                    UserName = question.User.Name
                });
            }

            return questionsDTO;
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

    }
}
