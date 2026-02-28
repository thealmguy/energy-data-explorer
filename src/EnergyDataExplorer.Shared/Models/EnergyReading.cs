using EnergyDataExplorer.Shared.Enums;

namespace EnergyDataExplorer.Shared.Models;

/// <summary>A single half-hourly energy consumption reading from the Octopus API.</summary>
public sealed record EnergyReading(
    DateTimeOffset IntervalStart,
    DateTimeOffset IntervalEnd,
    decimal ConsumptionKwh,
    FuelType FuelType
);
