using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly UsersService usersService;

        public AuthenticationController(UsersService usersService)
        {
            this.usersService = usersService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            return PartialView("_LoginModal");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string mail, string password, string url)
        {
            var user = usersService.ValidateAndGetUser(mail, password);

            if (user == null)
            {
                return Redirect(url);
            };

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("mail", user.Email),
                new Claim("userid", user.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuthentication");
            await HttpContext.SignInAsync("CookieAuthentication", new ClaimsPrincipal(claimsIdentity));

            LanguageServices.SetLanguage(Response, user.Language);

            return Redirect(url);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCrenedtials(string mail, string password)
        {
            if (usersService.ValidateUser(mail, password))
            {
                return new JsonResult(new { valid = true });
            }
            return new JsonResult(new { valid = false });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuthentication");
            LanguageServices.ClearLanguage(Response);
            return new JsonResult(new {LoggedOut= true});
        }
    }
}
