using Microsoft.AspNetCore.Mvc;
using WebApp.Context;

namespace WebApp.Models
{
    public class ReportPdf : Report
    {
        public ReportPdf(PortalAukcyjnyContext dbContext) : base(dbContext)
        {

        }
        public override byte[] SerializeToFile(List<string[]> data, string[] headers)
        {
            throw new NotImplementedException();
        }
    }
}
