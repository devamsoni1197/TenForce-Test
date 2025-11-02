using System.IO.Pipelines;
using System.Text.Json;
using TenForce_Initials.Config;
using TenForce_Initials.Domain;

namespace TenForce_Initials.Output
{
    public class FilerWriter: IOutputWriter
    {
        private readonly AppOptions _options;
        private readonly ILogger<FilerWriter> _log;

        public FilerWriter(AppOptions options, ILogger<FilerWriter> log)
        {
            _options = options;
            _log = log;
        }

        public async Task WritePlanetsWithMoonAveragesAsync(IEnumerable<Planet> planets, CancellationToken ct = default)
        {
            var outDir = _options.OutPutFolder ?? "output";
            Directory.CreateDirectory(outDir);

            var csvPath = Path.Combine(outDir, "planets_with_moon_avg.csv");
            _log.LogInformation("Writing CSV to {p}", csvPath);
            using (var writer = new StreamWriter(csvPath))
            using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteField("PlanetId");
                csv.WriteField("PlanetName");
                csv.WriteField("MoonCount");
                csv.WriteField("AverageMoonTempK");
                csv.WriteField("AverageMoonTempC");
                csv.NextRecord();

                foreach (var p in planets)
                {
                    var avgK = p.GetAverageMoonTemperature();
                    double? avgC = avgK.HasValue ? avgK.Value - 273.15 : null;
                    csv.WriteField(p.Id);
                    csv.WriteField(p.Name);
                    csv.WriteField(p.Moons.Count());
                    csv.WriteField(avgK.HasValue ? avgK.Value.ToString("F2") : "");
                    csv.WriteField(avgC.HasValue ? avgC.Value.ToString("F2") : "");
                    csv.NextRecord();
                }
            }

            var jsonPath = Path.Combine(outDir, "planets_with_moon_avg.json");
            _log.LogInformation("Writing JSON to {p}", jsonPath);

            var export = planets.Select(p => new
            {
                p.Id,
                p.Name,
                MoonCount = p.Moons.Count,
                AverageMoonTempK = p.GetAverageMoonTemperature(),
                AverageMoonTempC = p.GetAverageMoonTemperature() is double k ? (k - 273.15) : (double?)null,
                Moons = p.Moons.Select(m => new { m.id, m.Name, m.Temperature })
            });

            var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, json, ct);

            Console.WriteLine($"Files written to: {Path.GetFullPath(outDir)}");
        }
    }
}
