using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Tests;

public sealed class WeatherAggregationServiceTests
{
    private readonly WeatherAggregationService _sut = new();

    private static WeatherReading MakeReading(DateTimeOffset timestamp, double temp = 10.0) =>
        new(timestamp, temp, 70.0, 15.0, 30.0);

    [Fact]
    public void AggregatePeriods_Day_ComputesDailyAverages()
    {
        var day1 = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var readings = Enumerable.Range(0, 24)
            .Select(h => MakeReading(day1.AddHours(h), temp: 12.0))
            .ToList();

        var result = _sut.AggregatePeriods(readings, ComparisonPeriod.Day);

        Assert.Single(result);
        Assert.Equal(12.0, result[0].AvgTemperatureCelsius);
    }

    [Fact]
    public void BuildCorrelationPoints_MatchesDailyEnergyAndWeather()
    {
        var day = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);

        var energy = new List<EnergyReading>
        {
            new(day, day.AddMinutes(30), 3.0m, FuelType.Electricity)
        };

        var weather = Enumerable.Range(0, 24)
            .Select(h => MakeReading(day.AddHours(h), temp: 8.0))
            .ToList();

        var result = _sut.BuildCorrelationPoints(energy, weather, FuelType.Electricity);

        Assert.Single(result);
        Assert.Equal(3.0m, result[0].ConsumptionKwh);
        Assert.Equal(8.0, result[0].TemperatureCelsius);
    }

    [Fact]
    public void BuildCorrelationPoints_ExcludesDaysWithNoWeatherData()
    {
        var day1 = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);
        var day2 = new DateTimeOffset(2024, 4, 2, 0, 0, 0, TimeSpan.Zero);

        var energy = new List<EnergyReading>
        {
            new(day1, day1.AddMinutes(30), 3.0m, FuelType.Gas),
            new(day2, day2.AddMinutes(30), 4.0m, FuelType.Gas)
        };

        // Only weather for day1
        var weather = Enumerable.Range(0, 24)
            .Select(h => MakeReading(day1.AddHours(h)))
            .ToList();

        var result = _sut.BuildCorrelationPoints(energy, weather, FuelType.Gas);

        Assert.Single(result);
        Assert.Equal(day1.Date, result[0].Date.Date);
    }
}
