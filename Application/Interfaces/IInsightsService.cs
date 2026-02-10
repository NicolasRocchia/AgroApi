using APIAgroConnect.Contracts.Responses;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IInsightsService
    {
        Task<InsightsDto> GetInsightsAsync();
    }
}
