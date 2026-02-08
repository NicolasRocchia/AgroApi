using APIAgroConnect.Contracts.Models;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IRecipePdfParser
    {
        ParsedRecipe Parse(Stream pdfStream, string rawText);
    }
}