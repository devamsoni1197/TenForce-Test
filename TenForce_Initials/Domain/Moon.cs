namespace TenForce_Initials.Domain
{
    public class Moon
    {
        public string id { get; }
        public string Name { get; }
        public double? Temperature { get; }
        public Moon(string id, string name, double? temperature)
        {
            this.id = id;
            this.Name = name;
            this.Temperature = temperature;
        }
    }
}
