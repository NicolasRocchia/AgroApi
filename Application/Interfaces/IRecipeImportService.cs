namespace APIAgroConnect.Application.Interfaces
{
    public interface IRecipeImportService
    {
        Task<object> ImportAsync(IFormFile pdf, long actorUserId, bool dryRun);
    }
}
