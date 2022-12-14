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


        [HttpGet]
        [Route("/reports/csv/auctions/endedin/{timeSpan}")]
        public ActionResult AuctionEndedInGivenTimeCsv(int timeSpan = 7)
        {
            var report = new ReportCsv(dbContext);
            var file = report.GenerateAuctionsEndedInGivenTimeSpan(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);
            return File(file, "text/csv", $"AuctionEndedFrom_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.csv");
        }

        [HttpGet]
        [Route("/reports/pdf/auctions/endedin/{timeSpan}")]
        public ActionResult AuctionEndedInGivenTimePdf(int timeSpan = 7)
        {
            var report = new ReportPdf(dbContext);
            var file = report.GenerateAuctionsEndedInGivenTimeSpan(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);

            
            return File(file, "application/pdf", $"AuctionEndedFrom_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.pdf");
        }
    }
}
