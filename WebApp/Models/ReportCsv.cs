using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Text;
using WebApp.Context;

namespace WebApp.Models
{
    public class ReportCsv : Report
    {
        public ReportCsv(PortalAukcyjnyContext dbContext) : base(dbContext)
        {

        }
        public override byte[] SerializeToFile(List<string[]> data, string[] headers)
        {
            StringBuilder fileFormatBuilder = new StringBuilder();

            fileFormatBuilder.AppendLine(string.Join(';', headers));

            foreach(var line in data)
            {
                fileFormatBuilder.AppendLine(string.Join(';', line));
            }

            var entries = fileFormatBuilder.ToString();
           
            var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(entries));


            return Encoding.UTF8.GetBytes(entries);
        }
    }
}
