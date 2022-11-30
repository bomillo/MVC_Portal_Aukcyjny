using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AuctionQuestionsController : Controller
    {
        private readonly PortalAukcyjnyContext _context;

        public AuctionQuestionsController(PortalAukcyjnyContext context)
        {
            _context = context;
        }

        // GET: AuctionQuestions
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.AuctionQuestion.Include(a => a.Auction).Include(a => a.User);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: AuctionQuestions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AuctionQuestion == null)
            {
                return NotFound();
            }

            var auctionQuestion = await _context.AuctionQuestion
                .Include(a => a.Auction)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (auctionQuestion == null)
            {
                return NotFound();
            }

            return View(auctionQuestion);
        }

        // GET: AuctionQuestions/Create
        public IActionResult Create()
        {
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: AuctionQuestions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionId,UserId,AuctionId,Question,Answer,PublishedTime,AnsweredTime")] AuctionQuestion auctionQuestion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auctionQuestion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", auctionQuestion.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionQuestion.UserId);
            return View(auctionQuestion);
        }

        // GET: AuctionQuestions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AuctionQuestion == null)
            {
                return NotFound();
            }

            var auctionQuestion = await _context.AuctionQuestion.FindAsync(id);
            if (auctionQuestion == null)
            {
                return NotFound();
            }
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", auctionQuestion.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionQuestion.UserId);
            return View(auctionQuestion);
        }

        // POST: AuctionQuestions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuestionId,UserId,AuctionId,Question,Answer,PublishedTime,AnsweredTime")] AuctionQuestion auctionQuestion)
        {
            if (id != auctionQuestion.QuestionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auctionQuestion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionQuestionExists(auctionQuestion.QuestionId))
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
            ViewData["AuctionId"] = new SelectList(_context.Auctions, "AuctionId", "Title", auctionQuestion.AuctionId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", auctionQuestion.UserId);
            return View(auctionQuestion);
        }

        // GET: AuctionQuestions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AuctionQuestion == null)
            {
                return NotFound();
            }

            var auctionQuestion = await _context.AuctionQuestion
                .Include(a => a.Auction)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (auctionQuestion == null)
            {
                return NotFound();
            }

            return View(auctionQuestion);
        }

        // POST: AuctionQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AuctionQuestion == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.AuctionQuestion'  is null.");
            }
            var auctionQuestion = await _context.AuctionQuestion.FindAsync(id);
            if (auctionQuestion != null)
            {
                _context.AuctionQuestion.Remove(auctionQuestion);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ValidateQuestion(string question, string auctionId)
        {

            if (!HttpContext.User.Claims.Any())
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.NotLoggedIn });
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.InvalidQuestion });
            }

            if(question.Length > 1499)
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.FieldTooLong });
            }

            int auction = int.Parse(auctionId);

            if (!_context.Auctions.Any(i => i.AuctionId == auction))
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.SomethingWentWrong });
            }

            return new JsonResult(new { valid = true });
        }


        public async Task<ActionResult> ValidateQuestionAnswer(string answer, string questionId)
        {

            if (!HttpContext.User.Claims.Any())
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.NotLoggedIn });
            }


            if (string.IsNullOrWhiteSpace(answer))
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.InvalidAnswer });
            }

            int qId = int.Parse(questionId);
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value);

            if (!_context.AuctionQuestion.Any(a => a.QuestionId == qId))
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.SomethingWentWrong });
            }

            var auction = _context.AuctionQuestion.FirstOrDefault(a => a.QuestionId == qId);


            /*
            if(!_context.Auctions.Any(a => a.AuctionId == auction.AuctionId && a.OwnerId == userId))
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.NotOwner });
            }
            */
            if (answer.Length > 1499)
            {
                return new JsonResult(new { valid = false, message = WebApp.Resources.Shared.FieldTooLong });
            }

            return new JsonResult(new { valid = true });


        }

        public async Task<ActionResult> CreateQuestion(string question, string auctionId, string returnUrl)
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value);
            var auction = int.Parse(auctionId);

            _context.AuctionQuestion.Add(new AuctionQuestion()
            {
                UserId = userId,
                Question = question,
                AuctionId = auction,
                PublishedTime = DateTime.UtcNow
            });
            _context.SaveChanges();


            return Redirect(returnUrl);
        }

        public async Task<ActionResult> CreateAnswer(string answer, string questionId, string returnUrl)
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")).Value);
            var questionIdParsed = int.Parse(questionId);

            var question = _context.AuctionQuestion.Find(questionIdParsed);

            question.Answer = answer;
            question.AnsweredTime = DateTime.UtcNow;
            _context.Update(question);

            _context.SaveChanges();


            return Redirect(returnUrl);
        }
        private bool AuctionQuestionExists(int id)
        {
          return _context.AuctionQuestion.Any(e => e.QuestionId == id);
        }
    }
}
