using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class ElasticSearch : Controller
    {
        public GoogleRecaptchaService GoogleRecaptchaService { get; }

        public ElasticSearch(GoogleRecaptchaService googleRecaptchaService)
        {
            GoogleRecaptchaService = googleRecaptchaService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult GetSearchIdea()
        {
            return this.Content("");
        }

        public async Task<ActionResult> Search(string token)
        {
            var captchaResult = await GoogleRecaptchaService.VerifyToken(token);

            if (captchaResult)
            {
                //add some search entry to db
            }
            else if(!captchaResult && !string.IsNullOrWhiteSpace(token))
            {
                //weryfikacja nieudana, jakiś token był czyli nie zewnętrzne api => bot? XD
                return BadRequest();
            }


            return Redirect("~/");
        }
    }
}
