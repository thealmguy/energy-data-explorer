namespace EnergyDataExplorer.Shared.Models;

/// <summary>A single hourly weather observation or forecast.</summary>
public sealed record WeatherReading(
    DateTimeOffset Timestamp,
    double TemperatureCelsius,
    double RelativeHumidityPercent,
    double WindSpeedKmh,
    double SunshineDurationMinutes
);
