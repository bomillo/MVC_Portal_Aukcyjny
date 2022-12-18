using Elastic.Clients.Elasticsearch;
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
        public override string ConvertToFileFormat(List<string[]> data, string[] headers)
        {
            StringBuilder fileFormatBuilder = new StringBuilder();

            fileFormatBuilder.AppendLine(string.Join(';', headers));

            foreach(var line in data)
            {
                fileFormatBuilder.AppendLine(string.Join(';', line));
            }

            return fileFormatBuilder.ToString();

            
        }

        public override byte[] SerializeToFile(string fileData)
        {
            return Encoding.UTF8.GetBytes(fileData);
        }
    }
}
