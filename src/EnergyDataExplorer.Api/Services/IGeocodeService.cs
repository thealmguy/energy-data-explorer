namespace EnergyDataExplorer.Api.Services;

public interface IGeocodeService
{
    /// <summary>Resolves a UK postcode to latitude and longitude coordinates.</summary>
    Task<(double Latitude, double Longitude)> ResolvePostcodeAsync(
        string postcode,
        CancellationToken cancellationToken = default);
}
