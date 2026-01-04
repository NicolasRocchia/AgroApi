using APIAgroConnect.Contracts.Models;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IPdfLotsExtractor
    {
        Task<List<ParsedLot>> ExtractLotsAsync(Stream pdfStream);
    }
}
