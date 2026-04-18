namespace CarLookup.Core.Configuration;

public sealed class NhtsaOptions
{
    public const string SectionName = "Nhtsa";

    public string BaseUrl { get; set; } = "https://vpic.nhtsa.dot.gov/api/vehicles/";

    public int TimeoutSeconds { get; set; } = 15;

    public int MakesCacheMinutes { get; set; } = 1440;

    public int VehicleTypesCacheMinutes { get; set; } = 60;

    public int EarliestYear { get; set; } = 1995;
}
