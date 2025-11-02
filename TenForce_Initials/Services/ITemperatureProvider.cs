using static TenForce_Initials.Data.ApiDtos;

namespace TenForce_Initials.Services
{
    public interface ITemperatureProvider
    {
        //returns temperature in Kelvin for a mood body given its id
        Task<double?> GetMoonTemperatureKAsync(string moonId, string moonName, BodyDto parentPlaneDto);
    }
}
