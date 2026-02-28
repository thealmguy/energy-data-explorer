using EnergyDataExplorer.Shared.Enums;

namespace EnergyDataExplorer.Shared.Models;

/// <summary>
/// Side-by-side comparison of two discrete periods, with sub-period buckets on the X axis.
/// For Year: 12 monthly buckets. For Month: daily buckets. For Week: 7 daily buckets (Mon–Sun).
/// </summary>
public sealed record PeriodComparisonResult(
    ComparisonPeriod PeriodType,
    string PeriodALabel,
    string PeriodBLabel,
    IReadOnlyList<string> SubPeriodLabels,
    IReadOnlyList<decimal?> ElectricityA,
    IReadOnlyList<decimal?> ElectricityB,
    IReadOnlyList<decimal?> GasA,
    IReadOnlyList<decimal?> GasB);
