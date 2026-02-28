using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Services;

public sealed class WeatherAggregationService : IWeatherAggregationService
{
    public IReadOnlyList<WeatherSummary> AggregatePeriods(
        IReadOnlyList<WeatherReading> readings,
        ComparisonPeriod period)
    {
        var grouped = period switch
        {
            ComparisonPeriod.Day => readings
                .GroupBy(r => r.Timestamp.Date)
                .Select(g => BuildSummary(
                    g.Key.ToString("yyyy-MM-dd"),
                    new DateTimeOffset(g.Key, TimeSpan.Zero),
                    new DateTimeOffset(g.Key.AddDays(1), TimeSpan.Zero),
                    g.ToList()))
                .OrderBy(s => s.PeriodStart),

            ComparisonPeriod.Week => readings
                .GroupBy(r => GetIso8601WeekStart(r.Timestamp.Date))
                .Select(g => BuildSummary(
                    $"W/C {g.Key:dd MMM yyyy}",
                    new DateTimeOffset(g.Key, TimeSpan.Zero),
                    new DateTimeOffset(g.Key.AddDays(7), TimeSpan.Zero),
                    g.ToList()))
                .OrderBy(s => s.PeriodStart),

            ComparisonPeriod.Month => readings
                .GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month })
                .Select(g => BuildSummary(
                    new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    new DateTimeOffset(new DateTime(g.Key.Year, g.Key.Month, 1), TimeSpan.Zero),
                    new DateTimeOffset(new DateTime(g.Key.Year, g.Key.Month, 1).AddMonths(1), TimeSpan.Zero),
                    g.ToList()))
                .OrderBy(s => s.PeriodStart),

            ComparisonPeriod.Year => readings
                .GroupBy(r => r.Timestamp.Year)
                .Select(g => BuildSummary(
                    g.Key.ToString(),
                    new DateTimeOffset(new DateTime(g.Key, 1, 1), TimeSpan.Zero),
                    new DateTimeOffset(new DateTime(g.Key + 1, 1, 1), TimeSpan.Zero),
                    g.ToList()))
                .OrderBy(s => s.PeriodStart),

            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };

        return grouped.ToList();
    }

    public IReadOnlyList<CorrelationPoint> BuildCorrelationPoints(
        IReadOnlyList<EnergyReading> energyReadings,
        IReadOnlyList<WeatherReading> weatherReadings,
        FuelType fuelType)
    {
        // Aggregate energy to daily totals
        var dailyEnergy = energyReadings
            .Where(r => r.FuelType == fuelType)
            .GroupBy(r => r.IntervalStart.Date)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.ConsumptionKwh));

        // Aggregate weather to daily averages
        var dailyWeather = weatherReadings
            .GroupBy(r => r.Timestamp.Date)
            .ToDictionary(
                g => g.Key,
                g => (
                    AvgTemp: g.Average(r => r.TemperatureCelsius),
                    AvgHumidity: g.Average(r => r.RelativeHumidityPercent),
                    AvgWind: g.Average(r => r.WindSpeedKmh),
                    TotalSunshine: g.Sum(r => r.SunshineDurationMinutes)
                ));

        return dailyEnergy.Keys
            .Where(date => dailyWeather.ContainsKey(date))
            .Select(date => new CorrelationPoint(
                new DateTimeOffset(date, TimeSpan.Zero),
                dailyEnergy[date],
                fuelType,
                dailyWeather[date].AvgTemp,
                dailyWeather[date].AvgHumidity,
                dailyWeather[date].AvgWind,
                dailyWeather[date].TotalSunshine))
            .OrderBy(p => p.Date)
            .ToList();
    }

    private static WeatherSummary BuildSummary(
        string label,
        DateTimeOffset start,
        DateTimeOffset end,
        IReadOnlyList<WeatherReading> readings) =>
        new(
            label,
            start,
            end,
            readings.Average(r => r.TemperatureCelsius),
            readings.Average(r => r.RelativeHumidityPercent),
            readings.Average(r => r.WindSpeedKmh),
            readings.Sum(r => r.SunshineDurationMinutes));

    private static DateTime GetIso8601WeekStart(DateTime date)
    {
        var delta = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (delta < 0) delta += 7;
        return date.AddDays(-delta).Date;
    }
}
