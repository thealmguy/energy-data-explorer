using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Services;

public interface IWeatherService
{
    Task<IReadOnlyList<WeatherReading>> GetHistoricalWeatherAsync(
        double latitude,
        double longitude,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}
