using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TenForce_Initials.Data;
using TenForce_Initials.Domain;
using static TenForce_Initials.Data.ApiDtos;

namespace TenForce_Initials.Services
{
    public class DataProcessor : IDataProcessor
    {
        private readonly IApiClient _api;
        private readonly ILogger<IDataProcessor> _log;
        private readonly ITemperatureProvider _tempProvider;
        public DataProcessor(IApiClient api, ITemperatureProvider tempProvider, ILogger<DataProcessor> log)
        {
            _api = api;
            _tempProvider = tempProvider;
            _log = log;
        }
        public async Task<IEnumerable<Planet>> LoadPlanetsAsync(CancellationToken ct = default)
        {
            // throw new NotImplementedException();
            var bodies = (await _api.GetBodiesAsync(ct)).ToList();
            _log.LogInformation("Loaded {count} bodies from API", bodies.Count);

            // Build lookup by englishName and id
            var byId = bodies.Where(b => !string.IsNullOrEmpty(b.Id)).ToDictionary(b => b.Id, StringComparer.InvariantCultureIgnoreCase);
            var byName = bodies.Where(b => !string.IsNullOrEmpty(b.EnglishName)).ToDictionary(b => b.EnglishName, StringComparer.InvariantCultureIgnoreCase);

            var planetsDto = bodies.Where(b => (b.IsPlanet ?? false) || string.Equals(b.BodyType, "Planet", StringComparison.InvariantCultureIgnoreCase)).ToList();

            var planets = new List<Planet>();
            foreach (var pDto in planetsDto)
            {
                var p = new Planet(pDto.Id ?? pDto.EnglishName ?? Guid.NewGuid().ToString(), pDto.EnglishName ?? pDto.Name ?? "Unknown");
                // find moons: some moons are listed in moons[] of planet; others are separate bodies with aroundPlanet.planet == planet id.
                // 1) Use moons[] entries if present (names)
                var moonNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

                if (pDto.Moons != null)
                {
                    foreach (var mr in pDto.Moons)
                    {
                        if (!string.IsNullOrEmpty(mr.Moon))
                            moonNames.Add(mr.Moon);
                    }
                }

                // 2) Also find bodies that have aroundPlanet referencing this planet
                var candidateMoons = bodies.Where(b => b.AroundPlanet != null &&
                    string.Equals(b.AroundPlanet.Planet ?? "", pDto.Id ?? "", StringComparison.InvariantCultureIgnoreCase)).ToList();

                foreach (var m in candidateMoons)
                    if (!string.IsNullOrEmpty(m.EnglishName))
                        moonNames.Add(m.EnglishName);

                // Build Moon domain objects
                foreach (var moonName in moonNames)
                {
                    // try match DTO by name
                    BodyDto matched = null;
                    byName.TryGetValue(moonName, out matched);

                    string moonId = matched?.Id ?? moonName;
                    double? temp = null;

                    // 1st: if DTO has avgTemp use it
                    if (matched?.AvgTemp != null) temp = matched.AvgTemp;

                    // 2nd: else use temperature provider (which implements CSV fallback or estimator)
                    if (temp == null)
                    {
                        temp = await _tempProvider.GetMoonTemperatureKAsync(moonId, moonName, pDto);
                    }

                    p.Moons.Add(new Moon(moonId, moonName, temp));
                }

                planets.Add(p);
            }

            return planets;
        }
    }
}
