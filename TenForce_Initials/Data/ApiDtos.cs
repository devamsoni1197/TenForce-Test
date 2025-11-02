using System.Text.Json.Serialization;

namespace TenForce_Initials.Data
{
    public class ApiDtos
    {
        // partial DTO classes for API requests and responses
        public class ApiBodiesRoot
        {
            [JsonPropertyName("bodies")]
            public List<BodyDto> Bodies { get; set; } = new();
        }

        public class BodyDto
        {
            [JsonPropertyName("id")]
            public string Id { get; init; } = string.Empty;
            [JsonPropertyName("englishName")]
            public string EnglishName { get; init; } = string.Empty;
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("bodyType")]
            public string BodyType { get; init; } = string.Empty;
            [JsonPropertyName("isPlanet")]
            public bool? IsPlanet { get; init; }
            [JsonPropertyName("aroundPlanet")]
            public AroundPlanetDto AroundPlanet { get; init; }
            [JsonPropertyName("moons")]
            public List<MoonRefDto> Moons { get; set; }
            [JsonPropertyName("semimajorAxis")]
            public double? SemimajorAxis { get; set; }
            [JsonPropertyName("avgTemp")]
            public double? AvgTemp { get; set; }
        }
        public class AroundPlanetDto
        {
            [JsonPropertyName("planet")]
            public string Planet { get; init; } = string.Empty;
        }
        public class MoonRefDto
        {
            [JsonPropertyName("moon")]
            public string Moon { get; init; } = string.Empty;
        }
    }
}
