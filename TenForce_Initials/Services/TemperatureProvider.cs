using System.Runtime.CompilerServices;
using TenForce_Initials.Config;
using TenForce_Initials.Data;

namespace TenForce_Initials.Services
{
    public class TemperatureProvider : ITemperatureProvider
    {
            private readonly ILogger<TemperatureProvider> _log;
        private readonly IMoonTemperatureCsvProvider _csv;
        private readonly AppOptions _options;

        public TemperatureProvider(ILogger<TemperatureProvider> log, IMoonTemperatureCsvProvider moonTemperatureCsvProvider, AppOptions options)
        {
            _log = log;
            _csv = moonTemperatureCsvProvider;
            _options = options;
        }

        public Task<double?> GetMoonTemperatureKAsync(string moonId, string moonName, ApiDtos.BodyDto parentPlaneDto)
        {
        //throw new NotImplementedException();
        if(!_options.ForceEstimator)
            {
                var csvVal = _csv.GetTemperatureK(moonId) ?? _csv.GettemperatureKByName(moonName);
                if(csvVal != null)
                {
                    _log.LogInformation("Found Temp for moon {m} in CSV: {t} K", moonName, csvVal);
                    return Task.FromResult<double?>(csvVal);
                }
            }
            _log.LogDebug("No Temperature For Moon {m}", moonName);
            return Task.FromResult<double?>(null);
        }
 
    }
}
