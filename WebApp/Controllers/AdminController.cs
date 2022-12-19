using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Context;

namespace WebApp.Controllers
{
    [Authorize("RequireAdmin")]
    public class AdminController : Controller
    {
        private readonly PortalAukcyjnyContext dbContext;

        public AdminController(PortalAukcyjnyContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public ActionResult AdminPanel()
        {
            if (!HttpContext.User.Claims.Any())
            {
                return Redirect("/");
            }

            var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type.ToLower().Contains("userid")).Value);

            if (dbContext.Users.Single(x => x.UserId == id).UserType != Models.UserType.Admin)
            {
                return Redirect("/");
            }
            return View();
        }

    }
}
