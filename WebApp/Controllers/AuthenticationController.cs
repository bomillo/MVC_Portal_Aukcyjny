﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
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
        public async Task LoginGoogle(string url)
        {
            var authProp = new AuthenticationProperties()
            {
                RedirectUri = Url.Action("ExternalLoginCallback", new {url = url, provider = ExternalProvider.Google})
                
            };

            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, authProp);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task LoginFacebook(string url)
        {
            var authProp = new AuthenticationProperties()
            {
                RedirectUri = Url.Action("ExternalLoginCallback", new { url = url, provider = ExternalProvider.Facebook })

            };

            await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme, authProp);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string url, ExternalProvider provider)
        {
            var request = HttpContext.Request;

            var result = await HttpContext.AuthenticateAsync("CookieAuthentication");

            if (result.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }
            var externalUser = result.Principal;
            if (externalUser == null)
            {
                throw new Exception("External authentication error");
            }
            var claims = externalUser.Claims.ToList();
            var userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new Exception("Unknown userid");
            }
            var externalUserId = userIdClaim.Value;
            var externalProvider = userIdClaim.Issuer;

            var user = usersService.GetUserFromExternalProvider(externalUserId, provider);

            if(user == null)
            {
                user = ClaimsToData(claims, provider);
            }

            var ourClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("mail", user.Email),
                new Claim("userid", user.UserId.ToString())
            };

            await HttpContext.SignOutAsync();
            var claimsIdentity = new ClaimsIdentity(ourClaims, "CookieAuthentication");
            await HttpContext.SignInAsync("CookieAuthentication", new ClaimsPrincipal(claimsIdentity));

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

        private User ClaimsToData(List<Claim> claims, ExternalProvider provider)
        {
            if (claims == null || claims.Count == 0)
            {
                return null;
            };
            User user = new User()
            {
                Name = claims.FirstOrDefault(c => c.Type.ToLower().Contains("givenname")).Value.ToString(),
                Email = claims.FirstOrDefault(c => c.Type.ToLower().Contains("emailaddress")).Value.ToString(),
                ExternalProvider = provider,
                ExternalId = claims.FirstOrDefault(c => c.Type.ToLower().Contains("nameidentifier")).Value.ToString()
            };

            return usersService.AddUserFromExternalProvider(user);
        }
    }
}