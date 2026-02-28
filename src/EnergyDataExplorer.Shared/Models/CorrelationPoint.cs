using EnergyDataExplorer.Shared.Enums;

namespace EnergyDataExplorer.Shared.Models;

/// <summary>A single data point pairing energy consumption with a weather metric for scatter plot visualisation.</summary>
public sealed record CorrelationPoint(
    DateTimeOffset Date,
    decimal ConsumptionKwh,
    FuelType FuelType,
    double TemperatureCelsius,
    double RelativeHumidityPercent,
    double WindSpeedKmh,
    double SunshineDurationMinutes
);
