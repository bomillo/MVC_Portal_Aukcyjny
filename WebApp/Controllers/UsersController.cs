using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Models.DTO;
using WebApp.Resources.Authentication;
using WebApp.Services;
using static System.Collections.Specialized.BitVector32;

namespace WebApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly PortalAukcyjnyContext _context;
        private readonly UsersService _usersService;

        public UsersController(PortalAukcyjnyContext context, UsersService usersService)
        {
            _context = context;
            this._usersService = usersService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var portalAukcyjnyContext = _context.Users.Include(u => u.Company);
            return View(await portalAukcyjnyContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Email");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Name,Email,PasswordHashed,UserType,CompanyId,ThemeType,Language")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Email", user.CompanyId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Email", user.CompanyId);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Email,PasswordHashed,UserType,CompanyId,ThemeType,Language")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Email", user.CompanyId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'PortalAukcyjnyContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }


        // GET: Users/Details/5
        public async Task<IActionResult> UserAccount(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(m => m.UserId == id);

            ViewBag.User = user;

            var myAuctions = (from a in _context.Auctions
                             .Where(au => au.OwnerId == user.UserId &&
                                   (au.EndTime > DateTime.UtcNow || 
                                    au.IsDraft == true))
                             select a).ToList();

            ViewBag.MyAuctions = myAuctions;

            var myObservedAuctions = _context.ObservedAuctions.Where(x => x.UserId == id).ToList();
            if(myObservedAuctions != null)
            {
                List<DisplayAuctionsModel> observedAuctions = new List<DisplayAuctionsModel>();

                foreach(var myObserved in myObservedAuctions)
                {
                    var Auction = _context.Auctions.Where(x => x.AuctionId == myObserved.AuctionId &&
                                                          x.IsDraft == false).FirstOrDefault();

                    if(Auction == null)
                        { continue; }

                    var Bid = _context.Bid.Where(x => x.AuctionId == myObserved.AuctionId).ToList();
                    observedAuctions.Add(new DisplayAuctionsModel()
                    {
                        Auction = Auction,
                        Bid = Bid.Select(x => x.Price).DefaultIfEmpty(0).Max()

                    });
                }
                ViewBag.MyObservedAuctions = observedAuctions;
            }

            var myBids = _context.Bid.Where(x => x.UserId == id).Include(x => x.Auction).ToList();

            ViewBag.MyBids = myBids;

            return View();

        }
 
    
        // GET: Users/UserEdition/{id}
        public async Task<IActionResult> UserEdition(int? id, string? result)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = _context.Users
                .Where(x => x.UserId == id)
                .Include(x => x.Company)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            EditAccountModel editAccountModel = new EditAccountModel();

            ViewBag.User = user;
            ViewBag.Message = result;

            List<Language> languages = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();
            List<ThemeType> themes = Enum.GetValues(typeof(ThemeType)).Cast<ThemeType>().ToList();

            ViewData["Themes"]= new SelectList(themes, null, null, themes[user.ThemeType.GetHashCode()]);
            ViewData["Languages"]= new SelectList(languages, null, null, languages[user.Language.GetHashCode()]);
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Name", user.CompanyId);

            return View(editAccountModel);
        }

        // POST: Users/UserEdition/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdition(int? id, [Bind("Name,Email,OldPassword,Password,PasswordVerification,CompanyId,newThemeType,newLanguage")] EditAccountModel editedUser)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = _context.Users
               .Where(x => x.UserId == id)
               .Include(x => x.Company)
               .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            if (user.PasswordHashed != null)
            {
                if (editedUser.OldPassword != null)
                {
                    string hashedOldPasswd = _usersService.HashPassword(editedUser.OldPassword);

                    if (hashedOldPasswd != user.PasswordHashed)
                    {
                        ModelState.AddModelError("Password", WebApp.Resources.Authentication.Localization.PasswordNotMatch);

                        ViewBag.User = user;

                        List<Language> languages = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();
                        List<ThemeType> themes = Enum.GetValues(typeof(ThemeType)).Cast<ThemeType>().ToList();

                        ViewData["Languages"] = new SelectList(languages, null);
                        ViewData["Themes"] = new SelectList(themes, null);
                        ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Name", user.CompanyId);

                        return View(editedUser);
                    }

                    if (editedUser.Password != editedUser.PasswordVerification)
                    {
                        ModelState.AddModelError("Password", WebApp.Resources.Authentication.Localization.PasswordNotMatch);

                        ViewBag.User = user;

                        List<Language> languages = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();
                        List<ThemeType> themes = Enum.GetValues(typeof(ThemeType)).Cast<ThemeType>().ToList();

                        ViewData["Languages"] = new SelectList(languages, null);
                        ViewData["Themes"] = new SelectList(themes, null);
                        ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "Name", user.CompanyId);

                        return View(editedUser);
                    }

                    if (editedUser.Name != null)
                        user.Name = editedUser.Name;

                    if (editedUser.Email != null)
                        user.Email = editedUser.Email;

                    if (editedUser.Password != null)
                        user.PasswordHashed = _usersService.HashPassword(editedUser.Password);

                    if (editedUser.CompanyId != null)
                        user.CompanyId = editedUser.CompanyId;

                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim("mail", user.Email),
                        new Claim("userid", user.UserId.ToString())
                    };

                    HttpContext.Response.Cookies.Append("CookieAuthentication", null, new CookieOptions { Expires = DateTime.Now.AddDays(-1) });
                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuthentication");
                    await HttpContext.SignInAsync("CookieAuthentication", new ClaimsPrincipal(claimsIdentity));
                }
            }

            if (editedUser.newThemeType != null)
            {
                if (Request.Cookies["THEME_COOKIE"] != null)
                {
                    Response.Cookies.Delete("THEME_COOKIE");
                }

                user.ThemeType = (ThemeType)editedUser.newThemeType;

                var newTheme = (ThemeType)Enum.Parse(typeof(ThemeType), editedUser.newThemeType.ToString());

                HttpContext.Response.Cookies.Append("THEME_COOKIE", newTheme.ToString().ToLower(), new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1), IsEssential = true });

            }

            if (editedUser.newLanguage != null)
            {
                user.Language = (Language)editedUser.newLanguage;
                LanguageServices.SetLanguage(Response, user.Language);
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserEdition", new { id = user.UserId, result = Localization.EditionSuccess});
        }

        public string HashPassword(string password)
        {
            if (password != null)
                return _usersService.HashPassword(password);
            else 
                return password;
        }
    }
}
