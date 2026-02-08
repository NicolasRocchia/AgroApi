namespace APIAgroConnect.Application.Interfaces
{
    public interface IPdfTextExtractor
    {
        Task<string> ExtractTextAsync(Stream pdfStream);
    }
}
