using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
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
using Microsoft.Extensions.Caching.Memory;
using WebApp.Context;
using WebApp.Helpers;
using WebApp.Resources.Authentication;
using WebApp.Models;
using WebApp.Models.DTO;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly UsersService usersService;
        private readonly EmailService emailService;
        private readonly IMemoryCache memoryCache;

        public AuthenticationController(UsersService usersService, IMemoryCache memoryCache, EmailService emailService = null)
        {
            this.usersService = usersService;
            this.memoryCache = memoryCache;
            this.emailService = emailService;
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
        public async Task<IActionResult> SendResetPasswordMail(string mail)
        {
            var guid = Guid.NewGuid().ToString();
            memoryCache.Set(guid, mail, TimeSpan.FromMinutes(15));

            var user = usersService.GetUser(mail);
            if (user!=null) { 
                using (new CultureChanger(user))
                {
                    StringBuilder body = new StringBuilder(Mail.Body);
                    body.AppendLine();
                    body.AppendLine(Url.Action("ResetPassword", "Authentication", new {guid=guid}, Url.ActionContext.HttpContext.Request.Scheme));

                    emailService.SendMail(Mail.Subject, body.ToString(), user.Email);
                }
            }
            return View("ResetPasswordSent");
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string guid)
        {
            if (!memoryCache.TryGetValue(guid, out var _))
            {
                return LocalRedirect(Url.Action("Index", "Home"));
            }

            return View(new ResetPasswordModel() { Guid = guid });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (model.Password != model.PasswordVerification )
            {
                ModelState.AddModelError("Password", WebApp.Resources.Authentication.Localization.PasswordNotMatch);
            }
            string mail;
            if (!memoryCache.TryGetValue(model.Guid, out mail)) { 
                ModelState.AddModelError("Password", WebApp.Resources.Authentication.Localization.LinkExpired);
            }
            if (!ModelState.IsValid) {
                return View(model);
            }

            usersService.UpdatePassword(mail, model.Password);
            return View("ResetPasswordSent");
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
            return new JsonResult(new { LoggedOut = true });
        }


        [HttpGet]
        public async Task<IActionResult> Register()
        { 
            return View(new RegisterUserModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserModel model)
        {
            if (model.Password!= model.PasswordVerification) {
                ModelState.AddModelError("Password", WebApp.Resources.Authentication.Localization.PasswordNotMatch);
            }
            if (!usersService.CreateUser(model.Email, model.Name, model.Password)) {
                ModelState.AddModelError("Email", WebApp.Resources.Authentication.Localization.MailTaken);
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return View("RegisterSucces");
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
                Email = claims.FirstOrDefault(c => c.Type.ToLower().Contains("emailaddress")).Value.ToString()
            };

            if (provider == ExternalProvider.Facebook)
            {
                user.ExternalFacebookId = claims.FirstOrDefault(c => c.Type.ToLower().Contains("nameidentifier")).Value.ToString();
            }
            else
            {
                user.ExternalGoogleId = claims.FirstOrDefault(c => c.Type.ToLower().Contains("nameidentifier")).Value.ToString();

            }
            return usersService.AddUserFromExternalProvider(user);
        }
    }
}
