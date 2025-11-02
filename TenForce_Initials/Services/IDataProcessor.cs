namespace TenForce_Initials.Services
{
    public interface IDataProcessor
    {
        //load bodies from API and maps to domain planets with moons and temp.
        Task <IEnumerable<Domain.Planet>> LoadPlanetsAsync(CancellationToken ct = default); 
    }
}
