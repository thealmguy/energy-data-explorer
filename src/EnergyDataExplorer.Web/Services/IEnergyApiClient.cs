using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Web.Services;

public interface IEnergyApiClient
{
    Task<AppSettings?> GetServerSettingsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnergyReading>> GetConsumptionAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        FuelType? fuelType = null,
        CancellationToken cancellationToken = default);

    Task<PeriodComparisonResult> GetComparisonAsync(
        ComparisonPeriod period,
        DateTimeOffset periodAStart,
        DateTimeOffset periodBStart,
        FuelType? fuelType = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CorrelationPoint>> GetCorrelationAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        FuelType? fuelType = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WeatherReading>> GetWeatherSummaryAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<(double Latitude, double Longitude)> GeocodePostcodeAsync(
        string postcode,
        CancellationToken cancellationToken = default);
}
