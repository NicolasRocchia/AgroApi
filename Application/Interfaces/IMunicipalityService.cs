using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IMunicipalityService
    {
        // CRUD Municipios
        Task<List<MunicipalityDto>> GetAllAsync();
        Task<List<MunicipalityDto>> GetNearbyAsync(decimal latitude, decimal longitude, int limit = 10);
        Task<MunicipalityDto?> GetByIdAsync(long id);
        Task<MunicipalityDto> CreateAsync(CreateMunicipalityRequest request, long userId);
        Task UpdateAsync(long id, UpdateMunicipalityRequest request, long userId);

        // Flujo de asignación y revisión
        Task AssignToMunicipalityAsync(long recipeId, long municipalityId, long userId);
        Task ReviewRecipeAsync(long recipeId, ReviewRecipeRequest request, long municipalityId, long userId);

        // Mensajes
        Task<RecipeMessageDto> SendMessageAsync(long recipeId, string message, long userId);
        Task<List<RecipeMessageDto>> GetMessagesAsync(long recipeId);

        // Consultas del municipio
        Task<long?> GetMunicipalityIdByUserIdAsync(long userId);
    }
}
