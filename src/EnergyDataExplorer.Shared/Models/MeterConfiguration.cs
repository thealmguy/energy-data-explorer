namespace EnergyDataExplorer.Shared.Models;

/// <summary>Meter configuration for a single fuel type (electricity or gas).</summary>
public sealed record MeterConfiguration(
    string MeterPointReference,
    string SerialNumber
);
