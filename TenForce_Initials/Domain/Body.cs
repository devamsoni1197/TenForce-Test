namespace TenForce_Initials.Domain
{
    public class Body
    {
        public string id { get; init; }
        public string englishName { get; init; }
        public string bodyType { get; init; }
        public double? massValue { get; init; }
        public double? massExponent { get; init; }
        public double? semimajorAxis { get; init; }
        public List<string> moons { get; init; }
    }
}
