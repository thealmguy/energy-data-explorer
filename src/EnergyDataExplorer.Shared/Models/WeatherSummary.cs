namespace EnergyDataExplorer.Shared.Models;

/// <summary>Aggregated weather summary for a period (daily/weekly/monthly average).</summary>
public sealed record WeatherSummary(
    string PeriodLabel,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    double AvgTemperatureCelsius,
    double AvgRelativeHumidityPercent,
    double AvgWindSpeedKmh,
    double TotalSunshineDurationMinutes
);
