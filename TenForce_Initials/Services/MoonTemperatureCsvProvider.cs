using System.Globalization;

namespace TenForce_Initials.Services
{
    public class MoonTemperatureCsvProvider : IMoonTemperatureCsvProvider
    {
        private readonly Dictionary<string, double> _byId = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, double> _byName = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly ILogger<MoonTemperatureCsvProvider> _log;

        public MoonTemperatureCsvProvider(ILogger<MoonTemperatureCsvProvider> log)
        {
            _log = log;
            // Load CSV data into dictionaries
            // This is a placeholder for actual CSV loading logic
            // Example:
            // _byId["moon123"] = 120.0;
            // _byName["Europa"] = 102.0;
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "moon_temperatures.csv");
            if (!File.Exists(path))
            {
                _log.LogInformation("CSV file not found at path", path);
                return;
            }

            try
            {
                using var reader = new StreamReader(path);
                using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var id = csv.GetField("moonId");
                    var name = csv.GetField("moonName");
                    var tempStr = csv.GetField("temperatureK");
                    if (!double.TryParse(tempStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var t))
                        continue;
                    if (!string.IsNullOrEmpty(id)) _byId[id] = t;
                    if (!string.IsNullOrEmpty(name)) _byName[name] = t;

                }
                _log.LogInformation("Loaded {Count} moon temperatures from CSV", _byId.Count + _byName.Count);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error loading moon temperatures from CSV");
            }
        }
        public double? GetTemperatureK(string moonId)
        {
            //throw new NotImplementedException();
            return moonId != null && _byId.TryGetValue(moonId, out var temp) ? temp : null;
        }

        public double? GettemperatureKByName(string moonName)
        {
            return moonName != null && _byName.TryGetValue(moonName, out var temp) ? temp : null;
            //throw new NotImplementedException();
        }
    }
}
