using static TenForce_Initials.Data.ApiDtos;

namespace TenForce_Initials.Data
{
    public interface IApiClient
    {
        // returns raw body DTOs from the API
        Task<IEnumerable<BodyDto>> GetBodiesAsync(CancellationToken ct = default);
    }
}
