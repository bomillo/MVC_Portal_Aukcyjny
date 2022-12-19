﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PortalAukcyjny.Models;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;
using WebApp.Services;
using static System.Collections.Specialized.BitVector32;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApp.Controllers
{
    public class AuctionsController : Controller
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly BreadcrumbService breadcrumbService;
        private readonly AuctionFilesService _auctionFilesService;
        private readonly ObservAuctionService _observedAuctionService;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly AuctionEditHistoryService editHistoryService;
        private readonly BidsService bidsService;
        private readonly SetPagerService pagerService;
        private readonly AuctionsStatusService auctionsStatusService;

        public AuctionsController(PortalAukcyjnyContext context,
            ObservAuctionService observAuctionService,
            BreadcrumbService breadcrumbService,
            AuctionFilesService auctionFileService,
            BidsService bidsService,
            SetPagerService pagerService,
            ElasticsearchClient elasticsearchClient,
            AuctionEditHistoryService editHistoryService,
            AuctionsStatusService auctionsStatusService)
        {
            _context = context;
            this._observedAuctionService = observAuctionService;
            this.breadcrumbService = breadcrumbService;

            this.bidsService = bidsService;
            this.pagerService = pagerService;
            this._auctionFilesService = auctionFileService;

            _elasticsearchClient = elasticsearchClient;
            this.editHistoryService = editHistoryService;
            this.auctionsStatusService = auctionsStatusService;
        }

        // GET: Auctions
        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Index(int page = 1)
        {
            if(page < 1)
                page = 1;
            
            var portalAukcyjnyContext = _context.Auctions
                .Where(x => x.Status != AuctionStatus.Draft)
                .Include(a => a.Owner)
                .Include(a => a.Product).OrderBy(a => a.AuctionId);

            int userId = 0;
            int.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid"))?.Value, out userId);

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

                var Bid = bidsService.GetAuctionHighestBid(auction.AuctionId, userId);
                auctionList.Add(new DisplayAuctionsModel()
                {
                    Auction = auction,
                    iconPath = path,
                    Bid = Bid
                });
            }


            return View(auctionList);  // for displaying paged auctions
            
            //return View(await portalAukcyjnyContext.ToListAsync());   // for displaying all auctions
        }

        // GET: Auctions/Details/5
        [Authorize("RequireAdmin")]
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

            int userId;
            int.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid"))?.Value, out userId);
            ViewBag.IsObserved = _observedAuctionService.IsAuctionObserved((int)id, userId);

            if (auction == null)
            {
                return NotFound();
            }

            ViewBag.Breadcrumb = breadcrumbService.CreateCurrentPath(auction);

            return View(auction);
        }

        // GET: Auctions/Create
        [Authorize]
        public IActionResult Create()
        {
            //var userId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.ValueType == "userid").Value.ToString());
            var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid"))?.Value);

            ViewBag.OwnerId = userId;
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name");
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        [Authorize("RequireAdmin")]
        public async Task<IActionResult> Create([Bind("AuctionId,Description,Title,OwnerId,CreationTime,PublishedTime,EndTime,ProductId")] Auction auction, IFormCollection postedFiles, IFormFile productIcon, IFormFile productImage)
        {
            string[] descriptions = postedFiles["fileDescription"].ToString().Split(',');

            ModelState["Owner"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["Product"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productIcon"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            ModelState["productImage"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;

            auction.Status = AuctionStatus.Draft;
            auction.PublishedTime = null;
 
            if (auction.EndTime != null)
            {
                auction.EndTime = DateTime.SpecifyKind(auction.EndTime.Value, DateTimeKind.Utc);
            }

            if (auction.EndTime == null || DateTime.UtcNow > auction.EndTime)
            {
                ViewBag.DatesMsg = "End time is not valid!";
                ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
                return View(auction);
            }

            if (ModelState.IsValid)
            {
                auction.CreationTime = DateTime.SpecifyKind(DateTime.Now.ToUniversalTime(), DateTimeKind.Utc);

                _context.Add(auction);
                _elasticsearchClient.Index(new ElasticAuction { Id = auction.AuctionId, Title = auction.Title }, "auctions");
               

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
            var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value);
            
            ViewBag.OwnerId = userId;
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
            
            return View(auction);
        }

        // GET: Auctions/Edit/5
        [Authorize]
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

            if (auction.Status != AuctionStatus.Draft) {
                return RedirectToAction("Auction", "Auctions");
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

        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("AuctionId,Description,Title,OwnerId,CreationTime,PublishedTime,EndTime,ProductId")] Auction auction, IFormCollection postedFiles, IFormFile productIcon, IFormFile productImage)

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
                    if(auction.EndTime != null)
                    {
                        auction.EndTime = DateTime.SpecifyKind(auction.EndTime.Value, DateTimeKind.Utc);
                    }

                    if(auction.EndTime == null || DateTime.UtcNow > auction.EndTime)
                    {
                        ViewBag.DatesMsg = "End time is not valid!";
                        ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                        ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
                        return View(auction);
                    }

                    auction.PublishedTime = null;
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

                        ProductFile result = await _auctionFilesService.AddIcon(productIcon, auction);

                        if (result == null)
                        {
                            ViewBag.ErrorMessage = "Uneable to add productIcon file, please try again!";
                            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                            return View(auction);
                        }

                        await editHistoryService.AddChangesToHistory(auction, JsonConvert.SerializeObject(result));
                    }

                    if (productImage != null)
                    {
                        filesToSkip++;

                        ProductFile result = await _auctionFilesService.AddImage(productImage, auction);

                        if (result == null)
                        {
                            ViewBag.ErrorMessage = "Uneable to add productImage file, please try again!";
                            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                            return View(auction);
                        }

                        await editHistoryService.AddChangesToHistory(auction, JsonConvert.SerializeObject(result));
                    }

                    if (postedFiles != null)
                    {
                        int i = 0;
                        foreach (IFormFile file in postedFiles.Files)
                        {

                            if (filesToSkip-- > 0)
                                continue;

                            ProductFile result = await _auctionFilesService.AddOrdinaryFile(file, auction, descriptions[i++]);

                            if (result == null)
                            {
                                ViewBag.ErrorMessage = "Uneable to add productImage file, please try again!";
                                ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
                                ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", auction.ProductId);
                                return View(auction);
                            }

                            await editHistoryService.AddChangesToHistory(auction, JsonConvert.SerializeObject(result));
                        }
                    }

                    await editHistoryService.AddToHistoryChange(auction);

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
                return RedirectToAction("Auction", new { id = auction.AuctionId });
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Name", auction.OwnerId);
            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.Name), "ProductId", "Name", auction.ProductId);
            return View(auction);
        }

        // GET: Auctions/Delete/5
        [Authorize]

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
        [Authorize]
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
        [AllowAnonymous]
        public async Task<ActionResult> ObservAuction(int? id)
        {
            if (!HttpContext.User.Claims.Any())
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.NotLoggedIn });
            }
            string resultMsg = "";
            if(id > 0)
            {
                int userId = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value);

                if(userId > 0)
                {
                    var result = _observedAuctionService.Observe((int)id, userId);
                    if (result != null)
                    {
                        resultMsg = "Auction is now being observed";
                        ViewBag.IsObserved = true;
                        return new JsonResult(new { message = resultMsg });

                    }
                    resultMsg = "Auction already observed";
                    ViewBag.IsObserved = true;
                    return new JsonResult(new { message = resultMsg });

                }
            }
            resultMsg = "Invalid data, please try again";
            ViewBag.IsObserved = false;
            return new JsonResult(new { message = resultMsg });
        }
        [AllowAnonymous]
        public async Task<ActionResult> UnObservAuction(int? id, bool? ob)
        {
            if (!HttpContext.User.Claims.Any())
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.NotLoggedIn });
            }
            string resultMsg = "";
            int userId;
            int.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid"))?.Value, out userId);

            if (userId > 0)
            {
                bool result = _observedAuctionService.UnObserve((int)id, userId);
                if (result)
                {
                    resultMsg = "Auction is no longer being observed";
                    ViewBag.IsObserved = false;
                    return new JsonResult(new { message = resultMsg });
                }
                resultMsg = "Something went wrong, please try again!";
                ViewBag.IsObserved = false;
                return new JsonResult(new { message = resultMsg });
            }

            return new JsonResult(new { message = resultMsg });
        }
        [AllowAnonymous]
        public async Task<ActionResult> Auction(int? id)
        {
            if(id == null)
            {
                return BadRequest();
            }
            int userId;
            int.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid"))?.Value, out userId);

            var bidsTask = bidsService.GetAuctionBids((int)id, userId);
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
                Status = auction.Status,
                EndDate = auction.EndTime.GetValueOrDefault().ToString(),
                TimeToEnd = $"{WebApp.Resources.Shared.EndIn} {(auction.EndTime - DateTime.UtcNow).GetValueOrDefault().Days.ToString()} {WebApp.Resources.Shared.Days}"
            };
            auction.Product = _context.Products.FirstOrDefault(x => x.ProductId == auction.ProductId);
            ViewBag.Breadcrumb = breadcrumbService.CreateCurrentPath(auction);

            auctionDTO.Questions = await questionTask;
            auctionDTO.Bids = await bidsTask;

            //todo upload photo

            auctionDTO.Images = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("IMAGE")).Select(x => x.Path).ToList();


            if(auctionDTO.Images == null || auctionDTO.Images.Count() == 0)
            {
                auctionDTO.Images = new List<string>();
                auctionDTO.Images.Add(_auctionFilesService.GetErrorImagePath());
            };

            ViewBag.IsObserved = _observedAuctionService.IsAuctionObserved((int)id, userId);


            return View("Auction",auctionDTO);
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
        [AllowAnonymous]
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


        public async Task<IActionResult> Top()
        {
            var auctions = _context.Auctions;

            var bids = _context.Bid
                       .Include(x => x.Auction)
                       .Include(x => x.Auction.Product)
                       .GroupBy(x => x.AuctionId).Select(g => new
                                               {
                                                   AuctionId = g.Key,
                                                   Occurance = g.Count()
                                               }).OrderByDescending(a => a.Occurance).ToList().Take(10);

            List<DisplayAuctionsModel> TopAuctions = new List<DisplayAuctionsModel>();
            
            int userId = 0;
            int.TryParse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid"))?.Value, out userId);

            foreach (var bid in bids)
            {
                var auction = auctions.Where(x => x.AuctionId == bid.AuctionId)
                    .Include(x => x.Owner)    
                    .First();
                
                var auctionBid = bidsService.GetAuctionHighestBid(auction.AuctionId, userId);

                string path = null;
                var icon = _context.ProductFiles.Where(x => x.ProductId == auction.AuctionId && x.Name.StartsWith("ICON")).FirstOrDefault();

                if (icon != null)
                    path = icon.Path;
                else
                    path = _auctionFilesService.GetErrorIconPath();

                TopAuctions.Add(new DisplayAuctionsModel
                {
                    Auction = auction,
                    Bid = auctionBid,
                    iconPath = path
                });
            }

            return View(TopAuctions);
        }
        
        public async Task<ActionResult> Publish(int? id)
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

            if (DateTime.UtcNow < auction.EndTime)
            {
                auction.PublishedTime = DateTime.UtcNow;
                auction.Status = AuctionStatus.Published;
                _context.Update(auction);
                _context.SaveChanges();
                auctionsStatusService.RegisterNewAuctionForNotification(auction.AuctionId);
            }

            return await Auction(id);

        }
    }
}
