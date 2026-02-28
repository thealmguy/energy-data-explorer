using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Services;

public sealed class EnergyAggregationService : IEnergyAggregationService
{
    public IReadOnlyList<EnergyComparison> AggregatePeriods(
        IReadOnlyList<EnergyReading> readings,
        FuelType fuelType,
        ComparisonPeriod period)
    {
        var filtered = readings.Where(r => r.FuelType == fuelType).ToList();

        var grouped = period switch
        {
            ComparisonPeriod.Day => filtered
                .GroupBy(r => r.IntervalStart.Date)
                .Select(g => (Label: g.Key.ToString("yyyy-MM-dd"),
                              Start: new DateTimeOffset(g.Key, TimeSpan.Zero),
                              End: new DateTimeOffset(g.Key.AddDays(1), TimeSpan.Zero),
                              Total: g.Sum(r => r.ConsumptionKwh)))
                .OrderBy(x => x.Start),

            ComparisonPeriod.Week => filtered
                .GroupBy(r => GetIso8601WeekStart(r.IntervalStart.Date))
                .Select(g => (Label: $"W/C {g.Key:dd MMM yyyy}",
                              Start: new DateTimeOffset(g.Key, TimeSpan.Zero),
                              End: new DateTimeOffset(g.Key.AddDays(7), TimeSpan.Zero),
                              Total: g.Sum(r => r.ConsumptionKwh)))
                .OrderBy(x => x.Start),

            ComparisonPeriod.Month => filtered
                .GroupBy(r => new { r.IntervalStart.Year, r.IntervalStart.Month })
                .Select(g => (Label: new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                              Start: new DateTimeOffset(new DateTime(g.Key.Year, g.Key.Month, 1), TimeSpan.Zero),
                              End: new DateTimeOffset(new DateTime(g.Key.Year, g.Key.Month, 1).AddMonths(1), TimeSpan.Zero),
                              Total: g.Sum(r => r.ConsumptionKwh)))
                .OrderBy(x => x.Start),

            ComparisonPeriod.Year => filtered
                .GroupBy(r => r.IntervalStart.Year)
                .Select(g => (Label: g.Key.ToString(),
                              Start: new DateTimeOffset(new DateTime(g.Key, 1, 1), TimeSpan.Zero),
                              End: new DateTimeOffset(new DateTime(g.Key + 1, 1, 1), TimeSpan.Zero),
                              Total: g.Sum(r => r.ConsumptionKwh)))
                .OrderBy(x => x.Start),

            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };

        var periods = grouped.ToList();
        var comparisons = new List<EnergyComparison>(periods.Count);

        for (var i = 0; i < periods.Count; i++)
        {
            var (label, start, end, total) = periods[i];
            var prior = i > 0 ? periods[i - 1].Total : (decimal?)null;
            comparisons.Add(new EnergyComparison(label, start, end, total, fuelType, prior));
        }

        return comparisons;
    }

    public PeriodComparisonResult BuildPeriodComparison(
        IReadOnlyList<EnergyReading> periodAReadings,
        IReadOnlyList<EnergyReading> periodBReadings,
        ComparisonPeriod period,
        DateTimeOffset periodAStart,
        DateTimeOffset periodBStart)
    {
        string[] labels;
        Func<DateTimeOffset, int> getKey;

        switch (period)
        {
            case ComparisonPeriod.Year:
                labels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                getKey = r => r.Month - 1;
                break;

            case ComparisonPeriod.Month:
                var maxDays = Math.Max(
                    DateTime.DaysInMonth(periodAStart.Year, periodAStart.Month),
                    DateTime.DaysInMonth(periodBStart.Year, periodBStart.Month));
                labels = Enumerable.Range(1, maxDays).Select(d => d.ToString()).ToArray();
                getKey = r => r.Day - 1;
                break;

            case ComparisonPeriod.Week:
                labels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
                getKey = r => ((int)r.DayOfWeek + 6) % 7; // Mon=0 … Sun=6
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(period));
        }

        return new PeriodComparisonResult(
            period,
            GetPeriodLabel(period, periodAStart),
            GetPeriodLabel(period, periodBStart),
            labels,
            AggregateToBuckets(periodAReadings, FuelType.Electricity, getKey, labels.Length),
            AggregateToBuckets(periodBReadings, FuelType.Electricity, getKey, labels.Length),
            AggregateToBuckets(periodAReadings, FuelType.Gas, getKey, labels.Length),
            AggregateToBuckets(periodBReadings, FuelType.Gas, getKey, labels.Length));
    }

    private static decimal?[] AggregateToBuckets(
        IReadOnlyList<EnergyReading> readings,
        FuelType fuel,
        Func<DateTimeOffset, int> getKey,
        int bucketCount)
    {
        var buckets = new decimal?[bucketCount];
        foreach (var r in readings.Where(r => r.FuelType == fuel))
        {
            var key = getKey(r.IntervalStart);
            if (key >= 0 && key < bucketCount)
                buckets[key] = (buckets[key] ?? 0m) + r.ConsumptionKwh;
        }
        return buckets;
    }

    private static string GetPeriodLabel(ComparisonPeriod period, DateTimeOffset start) => period switch
    {
        ComparisonPeriod.Year  => start.Year.ToString(),
        ComparisonPeriod.Month => start.ToString("MMM yyyy"),
        ComparisonPeriod.Week  => $"W/C {start:dd MMM yyyy}",
        _ => start.ToString("yyyy-MM-dd")
    };

    private static DateTime GetIso8601WeekStart(DateTime date)
    {
        var delta = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (delta < 0) delta += 7;
        return date.AddDays(-delta).Date;
    }
}
