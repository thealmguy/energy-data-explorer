using EnergyDataExplorer.Shared.Enums;

namespace EnergyDataExplorer.Api.Models;

public sealed record ConsumptionRequest(
    string OctopusApiKey,
    string ElectricityMpan,
    string ElectricitySerial,
    string GasMprn,
    string GasSerial,
    DateTimeOffset From,
    DateTimeOffset To,
    FuelType? FuelType = null
);
