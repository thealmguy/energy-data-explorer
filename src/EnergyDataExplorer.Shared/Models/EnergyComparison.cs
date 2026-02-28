using EnergyDataExplorer.Shared.Enums;

namespace EnergyDataExplorer.Shared.Models;

/// <summary>Aggregated energy consumption for a named period, with an optional prior period for comparison.</summary>
public sealed record EnergyComparison(
    string PeriodLabel,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    decimal TotalKwh,
    FuelType FuelType,
    decimal? PriorPeriodKwh = null
)
{
    public decimal? ChangePercent =>
        PriorPeriodKwh.HasValue && PriorPeriodKwh.Value != 0
            ? Math.Round((TotalKwh - PriorPeriodKwh.Value) / PriorPeriodKwh.Value * 100, 1)
            : null;
}
