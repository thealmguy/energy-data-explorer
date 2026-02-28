using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Tests;

public sealed class EnergyAggregationServiceTests
{
    private readonly EnergyAggregationService _sut = new();

    private static IReadOnlyList<EnergyReading> MakeReadings(FuelType fuel, DateTimeOffset start, int halfHourCount, decimal kwhEach = 0.5m) =>
        Enumerable.Range(0, halfHourCount)
            .Select(i => new EnergyReading(
                start.AddMinutes(i * 30),
                start.AddMinutes((i + 1) * 30),
                kwhEach,
                fuel))
            .ToList();

    [Fact]
    public void AggregatePeriods_Day_GroupsByDate()
    {
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var readings = MakeReadings(FuelType.Electricity, start, 96); // 2 days of half-hours

        var result = _sut.AggregatePeriods(readings, FuelType.Electricity, ComparisonPeriod.Day);

        Assert.Equal(2, result.Count);
        Assert.Equal(24m, result[0].TotalKwh); // 48 half-hours × 0.5 kWh
    }

    [Fact]
    public void AggregatePeriods_Month_GroupsByMonth()
    {
        var jan = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var feb = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var readings = new List<EnergyReading>
        {
            new(jan, jan.AddMinutes(30), 1.0m, FuelType.Electricity),
            new(feb, feb.AddMinutes(30), 2.0m, FuelType.Electricity)
        };

        var result = _sut.AggregatePeriods(readings, FuelType.Electricity, ComparisonPeriod.Month);

        Assert.Equal(2, result.Count);
        Assert.Equal(1.0m, result[0].TotalKwh);
        Assert.Equal(2.0m, result[1].TotalKwh);
    }

    [Fact]
    public void AggregatePeriods_SetsChangePercent_ForSecondPeriod()
    {
        var jan = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var feb = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var readings = new List<EnergyReading>
        {
            new(jan, jan.AddMinutes(30), 100m, FuelType.Gas),
            new(feb, feb.AddMinutes(30), 110m, FuelType.Gas)
        };

        var result = _sut.AggregatePeriods(readings, FuelType.Gas, ComparisonPeriod.Month);

        Assert.Null(result[0].ChangePercent);
        Assert.Equal(10.0m, result[1].ChangePercent);
    }

    [Fact]
    public void AggregatePeriods_FiltersToRequestedFuelType()
    {
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var readings = new List<EnergyReading>
        {
            new(start, start.AddMinutes(30), 1.0m, FuelType.Electricity),
            new(start, start.AddMinutes(30), 5.0m, FuelType.Gas)
        };

        var result = _sut.AggregatePeriods(readings, FuelType.Electricity, ComparisonPeriod.Day);

        Assert.Single(result);
        Assert.Equal(1.0m, result[0].TotalKwh);
    }
}
