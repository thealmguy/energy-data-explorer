namespace EnergyDataExplorer.Shared.Models;

/// <summary>User-provided application settings: API credentials, location, and meter configuration.</summary>
public sealed class AppSettings
{
    public string OctopusApiKey { get; set; } = string.Empty;
    public string Postcode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public MeterConfiguration? ElectricityMeter { get; set; }
    public MeterConfiguration? GasMeter { get; set; }

    /// <summary>True when these settings were pre-loaded from server configuration rather than entered by the user.</summary>
    public bool IsFromServerConfig { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(OctopusApiKey)
        && !string.IsNullOrWhiteSpace(Postcode)
        && (ElectricityMeter != null || GasMeter != null);
}
