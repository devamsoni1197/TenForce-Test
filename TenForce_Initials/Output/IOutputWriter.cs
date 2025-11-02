using TenForce_Initials.Domain;

namespace TenForce_Initials.Output
{
    public interface IOutputWriter
    {
        Task WritePlanetsWithMoonAveragesAsync(IEnumerable<Planet> planets, CancellationToken ct = default);
    }
}
