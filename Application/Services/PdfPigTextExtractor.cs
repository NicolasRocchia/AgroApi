using APIAgroConnect.Application.Interfaces;
using System.Text;
using UglyToad.PdfPig;

namespace APIAgroConnect.Application.Services
{
    public sealed class PdfPigTextExtractor : IPdfTextExtractor
    {
        public Task<string> ExtractTextAsync(Stream pdfStream)
        {
            using var doc = PdfDocument.Open(pdfStream);
            var sb = new StringBuilder();

            foreach (var page in doc.GetPages())
            {
                sb.AppendLine(page.Text);
                sb.AppendLine("-----PAGE-----");
            }

            return Task.FromResult(sb.ToString());
        }
    }
}
