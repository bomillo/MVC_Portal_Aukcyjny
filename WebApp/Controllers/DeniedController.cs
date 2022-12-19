using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [AllowAnonymous]
    public class DeniedController : Controller
    {

        public IActionResult Denied(string returnurl)
        {
            return View();
        }
    }
}
