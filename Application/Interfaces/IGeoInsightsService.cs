using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;

namespace APIAgroConnect.Application.Interfaces
{
    public interface IGeoInsightsService
    {
        Task<GeoInsightsResponse> GetGeoInsightsAsync(long municipalityId, GeoInsightsRequest request);
    }
}
