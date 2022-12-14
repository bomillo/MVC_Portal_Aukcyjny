using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class ReportsController : Controller
    {
        private readonly PortalAukcyjnyContext dbContext;

        public ReportsController(PortalAukcyjnyContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var report = new ReportCsv(dbContext);
            var file = report.GenerateAuctionsEndedInGivenTimeSpan(20);

            return File(file, "text/csv");
        }
    }
}
