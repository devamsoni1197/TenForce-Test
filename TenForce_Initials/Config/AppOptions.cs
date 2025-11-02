namespace TenForce_Initials.Config
{
    public record AppOptions
    {
        //bundling into one object
        public string OutPutFolder { get; init; } = "Output";
        public bool ForceEstimator { get; init; } = false;
    }
}
