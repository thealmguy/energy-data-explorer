using System.Net.Http.Json;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;
using EnergyDataExplorer.Shared.State;

namespace EnergyDataExplorer.Web.Services;

public sealed class EnergyApiClient(HttpClient httpClient, AppSettingsState settingsState) : IEnergyApiClient
{
    public async Task<AppSettings?> GetServerSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("api/settings", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AppSettings>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<EnergyReading>> GetConsumptionAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        FuelType? fuelType = null,
        CancellationToken cancellationToken = default)
    {
        var settings = settingsState.Current;
        var request = new
        {
            settings.OctopusApiKey,
            ElectricityMpan = settings.ElectricityMeter?.MeterPointReference ?? string.Empty,
            ElectricitySerial = settings.ElectricityMeter?.SerialNumber ?? string.Empty,
            GasMprn = settings.GasMeter?.MeterPointReference ?? string.Empty,
            GasSerial = settings.GasMeter?.SerialNumber ?? string.Empty,
            From = from,
            To = to,
            FuelType = fuelType
        };

        var response = await httpClient.PostAsJsonAsync("api/energy/consumption", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<EnergyReading>>(cancellationToken) ?? [];
    }

    public async Task<PeriodComparisonResult> GetComparisonAsync(
        ComparisonPeriod period,
        DateTimeOffset periodAStart,
        DateTimeOffset periodBStart,
        FuelType? fuelType = null,
        CancellationToken cancellationToken = default)
    {
        var settings = settingsState.Current;
        var request = new
        {
            settings.OctopusApiKey,
            ElectricityMpan = settings.ElectricityMeter?.MeterPointReference ?? string.Empty,
            ElectricitySerial = settings.ElectricityMeter?.SerialNumber ?? string.Empty,
            GasMprn = settings.GasMeter?.MeterPointReference ?? string.Empty,
            GasSerial = settings.GasMeter?.SerialNumber ?? string.Empty,
            Period = period,
            PeriodAStart = periodAStart,
            PeriodBStart = periodBStart,
            FuelType = fuelType
        };

        var response = await httpClient.PostAsJsonAsync("api/energy/comparison", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PeriodComparisonResult>(cancellationToken)
            ?? throw new InvalidOperationException("Unexpected null comparison response.");
    }

    public async Task<IReadOnlyList<CorrelationPoint>> GetCorrelationAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        FuelType? fuelType = null,
        CancellationToken cancellationToken = default)
    {
        var settings = settingsState.Current;
        var request = new
        {
            settings.OctopusApiKey,
            ElectricityMpan = settings.ElectricityMeter?.MeterPointReference ?? string.Empty,
            ElectricitySerial = settings.ElectricityMeter?.SerialNumber ?? string.Empty,
            GasMprn = settings.GasMeter?.MeterPointReference ?? string.Empty,
            GasSerial = settings.GasMeter?.SerialNumber ?? string.Empty,
            settings.Latitude,
            settings.Longitude,
            From = from,
            To = to,
            FuelType = fuelType
        };

        var response = await httpClient.PostAsJsonAsync("api/energy/correlation", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CorrelationPoint>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<WeatherReading>> GetWeatherSummaryAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var settings = settingsState.Current;
        var request = new
        {
            settings.Latitude,
            settings.Longitude,
            From = from,
            To = to
        };

        var response = await httpClient.PostAsJsonAsync("api/weather/summary", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<WeatherReading>>(cancellationToken) ?? [];
    }

    public async Task<(double Latitude, double Longitude)> GeocodePostcodeAsync(
        string postcode,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/weather/geocode",
            new { Postcode = postcode },
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeocodeResult>(cancellationToken)
            ?? throw new InvalidOperationException("Unexpected null geocode response.");

        return (result.Latitude, result.Longitude);
    }

    private sealed record GeocodeResult(double Latitude, double Longitude);
}
