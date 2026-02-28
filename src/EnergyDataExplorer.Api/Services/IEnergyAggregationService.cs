using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Services;

public interface IEnergyAggregationService
{
    IReadOnlyList<EnergyComparison> AggregatePeriods(
        IReadOnlyList<EnergyReading> readings,
        FuelType fuelType,
        ComparisonPeriod period);

    PeriodComparisonResult BuildPeriodComparison(
        IReadOnlyList<EnergyReading> periodAReadings,
        IReadOnlyList<EnergyReading> periodBReadings,
        ComparisonPeriod period,
        DateTimeOffset periodAStart,
        DateTimeOffset periodBStart);
}
