using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Services;

public interface IOctopusEnergyService
{
    Task<IReadOnlyList<EnergyReading>> GetConsumptionAsync(
        string apiKey,
        MeterConfiguration meter,
        FuelType fuelType,
        DateTimeOffset from,
        DateTimeOffset to,
        string? groupBy = null,
        CancellationToken cancellationToken = default);
}
