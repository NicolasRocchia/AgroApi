using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IRecipeService
    {
        Task<PagedResponse<RecipeListItemDto>> GetRecipesAsync(RecipeQueryRequest request);
        Task<RecipeDetailDto?> GetRecipeByIdAsync(long id);
    }
}
