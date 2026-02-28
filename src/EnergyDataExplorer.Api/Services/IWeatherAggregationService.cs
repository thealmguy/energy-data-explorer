using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Services;

public interface IWeatherAggregationService
{
    IReadOnlyList<WeatherSummary> AggregatePeriods(
        IReadOnlyList<WeatherReading> readings,
        ComparisonPeriod period);

    IReadOnlyList<CorrelationPoint> BuildCorrelationPoints(
        IReadOnlyList<EnergyReading> energyReadings,
        IReadOnlyList<WeatherReading> weatherReadings,
        FuelType fuelType);
}
