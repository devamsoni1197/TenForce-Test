
using System.Text.Json;

namespace TenForce_Initials.Data
{
    public class ApiClient : IApiClient
    {
        public readonly HttpClient _httpClient;
        public readonly ILogger<ApiClient> _log;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> log)
        {
            _httpClient = httpClient;
            _log = log;
        }
        public async Task<IEnumerable<ApiDtos.BodyDto>> GetBodiesAsync(CancellationToken ct = default)
        {
            _log.LogInformation("Getting bodies from API");
            using var response = _httpClient.GetAsync("/rest/", ct);
            response.Wait(ct);
            if( response.Result.StatusCode != System.Net.HttpStatusCode.OK )
            {
                _log.LogError("Error getting bodies from API: {StatusCode}", response.Result.StatusCode);
                return Enumerable.Empty<ApiDtos.BodyDto>();
            }
            var stream = response.Result.Content.ReadAsStreamAsync(ct);
            var doc = await JsonSerializer.DeserializeAsync<ApiDtos.ApiBodiesRoot>(await stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true},ct);

            if(doc.Bodies == null) return Enumerable.Empty<ApiDtos.BodyDto>();
            return doc.Bodies;
            //throw new NotImplementedException();
        }
    }
}
