namespace TenForce_Initials.Domain
{
    public class Planet
    {
        public string Id { get; }
        public string Name { get; }
        public List<Moon> Moons { get; } = new();
        public Planet(string id, string name) { Id = id; Name = name; }
        public double? GetAverageMoonTemperature()
        {
            var moonTemperatures = Moons
                .Where(m => m.Temperature.HasValue)
                .Select(m => m.Temperature!.Value)
                .ToList();
            if (!moonTemperatures.Any())
                return null;
            return moonTemperatures.Average();
        }

    }
}
