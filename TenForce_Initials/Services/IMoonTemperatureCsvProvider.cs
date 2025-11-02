namespace TenForce_Initials.Services
{
    public interface IMoonTemperatureCsvProvider
    {
        double? GetTemperatureK(string moonId);
        double? GettemperatureKByName(string moonName);
    }
}
