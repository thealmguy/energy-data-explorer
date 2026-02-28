using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EnergyDataExplorer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SettingsController(
    IConfiguration configuration,
    IGeocodeService geocodeService) : ControllerBase
{
    /// <summary>
    /// Returns settings pre-configured via application configuration / user secrets.
    /// Returns an empty AppSettings (IsFromServerConfig = false) if nothing is configured.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AppSettings>> GetSettings(CancellationToken cancellationToken)
    {
        var section = configuration.GetSection("EnergySettings");

        var apiKey          = section["OctopusApiKey"]   ?? string.Empty;
        var postcode        = section["Postcode"]         ?? string.Empty;
        var latitude        = section.GetValue<double>("Latitude");
        var longitude       = section.GetValue<double>("Longitude");
        var electricityMpan = section["ElectricityMpan"] ?? string.Empty;
        var electricitySerial = section["ElectricitySerial"] ?? string.Empty;
        var gasMprn         = section["GasMprn"]         ?? string.Empty;
        var gasSerial       = section["GasSerial"]       ?? string.Empty;

        // Nothing configured — return empty so the frontend falls back to the Settings page.
        if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(postcode))
            return Ok(new AppSettings());

        // Auto-geocode if postcode is set but coordinates are absent.
        if (!string.IsNullOrWhiteSpace(postcode) && latitude == 0 && longitude == 0)
        {
            try
            {
                (latitude, longitude) = await geocodeService.ResolvePostcodeAsync(postcode, cancellationToken);
            }
            catch
            {
                // Coordinates will remain 0; the frontend will prompt the user to look up.
            }
        }

        var settings = new AppSettings
        {
            OctopusApiKey    = apiKey,
            Postcode         = postcode,
            Latitude         = latitude,
            Longitude        = longitude,
            ElectricityMeter = !string.IsNullOrWhiteSpace(electricityMpan)
                ? new MeterConfiguration(electricityMpan, electricitySerial)
                : null,
            GasMeter         = !string.IsNullOrWhiteSpace(gasMprn)
                ? new MeterConfiguration(gasMprn, gasSerial)
                : null,
            IsFromServerConfig = true
        };

        return Ok(settings);
    }
}
