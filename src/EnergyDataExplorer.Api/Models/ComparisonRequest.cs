using EnergyDataExplorer.Shared.Enums;

namespace EnergyDataExplorer.Api.Models;

public sealed record ComparisonRequest(
    string OctopusApiKey,
    string ElectricityMpan,
    string ElectricitySerial,
    string GasMprn,
    string GasSerial,
    ComparisonPeriod Period,
    DateTimeOffset PeriodAStart,
    DateTimeOffset PeriodBStart,
    FuelType? FuelType = null
);
