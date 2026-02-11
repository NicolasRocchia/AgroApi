using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;
using APIAgroConnect.Domain.Entities;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IRecipeService
    {
        Task<PagedResponse<RecipeListItemDto>> GetRecipesAsync(RecipeQueryRequest request);
        Task<RecipeDetailDto?> GetRecipeByIdAsync(long id);
        Task ChangeStatusAsync(long recipeId, string newStatus, long userId);
        Task<Municipality?> GetMunicipalityByUserIdAsync(long userId);
    }
}
