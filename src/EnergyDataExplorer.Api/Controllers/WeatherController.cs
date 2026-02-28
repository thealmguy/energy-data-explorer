using EnergyDataExplorer.Api.Models;
using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EnergyDataExplorer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WeatherController(
    IWeatherService weatherService,
    IGeocodeService geocodeService) : ControllerBase
{
    /// <summary>Returns aggregated weather summaries for a date range and period type.</summary>
    [HttpPost("summary")]
    public async Task<ActionResult<IReadOnlyList<WeatherSummary>>> GetSummary(
        [FromBody] WeatherRequest request,
        CancellationToken cancellationToken)
    {
        var readings = await weatherService.GetHistoricalWeatherAsync(
            request.Latitude,
            request.Longitude,
            request.From,
            request.To,
            cancellationToken);

        return Ok(readings);
    }

    /// <summary>Resolves a UK postcode to latitude/longitude coordinates.</summary>
    [HttpPost("geocode")]
    public async Task<ActionResult<object>> GeocodePostcode(
        [FromBody] PostcodeRequest request,
        CancellationToken cancellationToken)
    {
        var (lat, lon) = await geocodeService.ResolvePostcodeAsync(request.Postcode, cancellationToken);
        return Ok(new { Latitude = lat, Longitude = lon });
    }
}
