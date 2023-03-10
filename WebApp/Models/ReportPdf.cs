using DinkToPdf;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

using System.Text;
using WebApp.Context;

namespace WebApp.Models
{
    public class ReportPdf : Report
    {
        public ReportPdf(PortalAukcyjnyContext dbContext) : base(dbContext)
        {

        }
        public override string ConvertToFileFormat(List<string[]> data, string[] headers)
        {
            StringBuilder formatBuilder = new StringBuilder();


            foreach(var item in data)
            {
                for(int i = 0; i < item.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(item[i]))
                    {
                        formatBuilder.AppendLine($"<b>{headers[i]}</b>: {item[i]}<br/>");
                    }
                }
                formatBuilder.AppendLine("<br/>");
            }
            return formatBuilder.ToString();
        }

        public override byte[] SerializeToFile(string fileData)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4Plus


                },
                Objects =
                {
                    new ObjectSettings()
                    {

                        PagesCount = true,
                        HtmlContent = fileData,
                        WebSettings = {DefaultEncoding = "utf-8", MinimumFontSize = 20},
                        HeaderSettings = {FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812}
                    }
                }
            };

            var converter = new BasicConverter(new PdfTools());

            byte[] pdf = converter.Convert(doc);


            return pdf;
        }
    }
}
