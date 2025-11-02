using TenForce_Initials.Domain;

namespace TenForce_Initials.Output
{
    public class ConsoleWriter : IOutputWriter
    {
        public Task WritePlanetsWithMoonAveragesAsync(IEnumerable<Planet> planets, CancellationToken ct = default)
        {
            //throw new NotImplementedException();
            Console.WriteLine();
            Console.WriteLine("Planet: | #Moons | Avg Moon Temp (K) | Avg Moon Temp (°C)");
            Console.WriteLine(new string('-', 70));
            foreach (var p in planets.OrderBy(p => p.Name))
            {
                var avgK = p.GetAverageMoonTemperature();
                var avgC = avgK.HasValue ? avgK.Value - 273.15 : (double?)null;
                Console.WriteLine($"{p.Name.PadRight(26)} | {p.Moons.Count().ToString().PadLeft(6)} | {(avgK.HasValue ? avgK.Value.ToString("F2").PadLeft(16) : "N/A".PadLeft(16))} | {(avgC.HasValue ? avgC.Value.ToString("F2") : "N/A")}");
            }
            Console.WriteLine();
            return Task.CompletedTask;
        }
    }
}
