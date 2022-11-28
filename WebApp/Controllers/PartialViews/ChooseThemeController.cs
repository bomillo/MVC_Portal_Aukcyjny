﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Net.Http;
using WebApp.Context;
using WebApp.Extensions;
using WebApp.Models;

namespace WebApp.Controllers.PartialViews
{
    public class ChooseThemeController : Controller
    {

        public int Id { get; set; }

        private readonly PortalAukcyjnyContext _dbContext;
        public ChooseThemeController(PortalAukcyjnyContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ValueChanged(string themeType)
        {
            
            var newTheme = (ThemeType)Enum.Parse(typeof(ThemeType), themeType);

            if (HttpContext.User.Claims.Any())
            {
                if (HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("userid")) != null)
                {
                    var userId = int.Parse(HttpContext.User.Claims.SingleOrDefault(x => x.Type.ToLower().Contains("userid")).Value);
                    var user = _dbContext.Users.FirstOrDefault(x => x.UserId == userId);
                    if (user != null)
                    {
                        user.ThemeType = newTheme;
                        _dbContext.Update(user);
                        _dbContext.SaveChanges();
                    }
                }
            }
            if(Request.Cookies["THEME_COOKIE"] != null)
            {
                Response.Cookies.Delete("THEME_COOKIE");
            }

                HttpContext.Response.Cookies.Append("THEME_COOKIE", newTheme.ToString().ToLower(), new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1), IsEssential = true });

            return Redirect(HttpContext.Request.Headers.Referer.ToString());
        }
    }
}
