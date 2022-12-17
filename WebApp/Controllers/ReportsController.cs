using Microsoft.AspNetCore.Mvc;
using System;
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
        [Route("/reports/csv/auctions/ended")]
        public ActionResult AuctionEndedInGivenTimeCsv(int timeSpan = 3)
        {
            var report = new ReportCsv(dbContext);
            var file = report.GenerateAuctionsEndedInGivenTimeSpan(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);
            return File(file, "text/csv", $"AuctionEndedFrom_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.csv");
        }

        [HttpGet]
        [Route("/reports/pdf/auctions/ended")]
        public ActionResult AuctionEndedInGivenTimePdf(int timeSpan = 3)
        {
            var report = new ReportPdf(dbContext);
            var file = report.GenerateAuctionsEndedInGivenTimeSpan(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);

            
            return File(file, "application/pdf", $"AuctionEndedFrom_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.pdf");
        }

        [HttpGet]
        [Route("/reports/pdf/categories/popularity")]
        public ActionResult CategoriesPopularity(int timeSpan = 3)
        {
            var report = new ReportPdf(dbContext);
            var file = report.GenerateCategoryPopularityInDaySpan(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);

            return File(file, "application/pdf", $"CategoriesPopularity_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.pdf");
        }

        [HttpGet]
        [Route("/reports/csv/categories/popularity")]
        public ActionResult CategoriesPopularityCsv(int timeSpan = 3)
        {
            var report = new ReportCsv(dbContext);
            var file = report.GenerateCategoryPopularityInDaySpan(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);

            return File(file, "text/csv", $"CategoriesPopularity_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.csv");
        }

        [HttpGet]
        [Route("/reports/pdf/business")]
        public ActionResult Business(int timeSpan = 3)
        {
            var report = new ReportPdf(dbContext);
            var file = report.GenerateBusinessReport(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);

            return File(file, "application/pdf", $"Business_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.pdf");
        }

        [HttpGet]
        [Route("/reports/csv/business")]
        public ActionResult BusinessCsv(int timeSpan = 3)
        {
            var report = new ReportCsv(dbContext);
            var file = report.GenerateBusinessReport(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);

            return File(file, "text/csv", $"Business_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.csv");
        }

        [HttpGet]
        [Route("/reports/csv/auctions/created")]
        public ActionResult AuctionCreatedInGivenTimeCsv(int timeSpan = 3)
        {
            var report = new ReportCsv(dbContext);
            var file = report.GenerateNewAuctionsReport(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);
            return File(file, "text/csv", $"AuctionCreatedFrom_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.csv");
        }

        [HttpGet]
        [Route("/reports/pdf/auctions/created")]
        public ActionResult AuctionCreatedInGivenTimePdf(int timeSpan = 3)
        {
            var report = new ReportPdf(dbContext);
            var file = report.GenerateNewAuctionsReport(timeSpan);
            var startDate = DateTime.UtcNow.AddDays(timeSpan * -1);


            return File(file, "application/pdf", $"AuctionCreatedFrom_{startDate.Day}.{startDate.Month}.{startDate.Year}_{DateTime.UtcNow.Day}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Year}.pdf");
        }

        [HttpGet]
        [Route("/reports/pdf/auctions/history/my")]
        public ActionResult MyAuctionsHistoryPdf(int userId)
        {
            var report = new ReportPdf(dbContext);
            var file = report.GenerateMyAuctionHistoryReport(userId);


            return File(file, "application/pdf", $"MyAuctionHistory.pdf");
        }

        [HttpGet]
        [Route("/reports/csv/auctions/history/my")]
        public ActionResult MyAuctionsHistoryCsv(int userId)
        {
            var report = new ReportCsv(dbContext);
            var file = report.GenerateMyAuctionHistoryReport(userId);


            return File(file, "text/csv", $"MyAuctionHistory.csv");
        }

    }
}
