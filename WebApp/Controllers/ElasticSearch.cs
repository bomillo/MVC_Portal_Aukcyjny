using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class ElasticSearch : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult GetSearchIdea()
        {
            return this.Content("");
        }
    }
}
