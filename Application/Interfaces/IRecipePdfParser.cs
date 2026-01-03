using APIAgroConnect.Contracts.Models;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IRecipePdfParser
    {
        ParsedRecipe Parse(string pdfText);
    }
}