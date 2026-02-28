using EnergyDataExplorer.Shared.Enums;

namespace EnergyDataExplorer.Api.Models;

public sealed record CorrelationRequest(
    string OctopusApiKey,
    string ElectricityMpan,
    string ElectricitySerial,
    string GasMprn,
    string GasSerial,
    double Latitude,
    double Longitude,
    DateTimeOffset From,
    DateTimeOffset To,
    FuelType? FuelType = null
);
