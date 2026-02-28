using EnergyDataExplorer.Api.Models;
using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EnergyDataExplorer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EnergyController(
    IOctopusEnergyService octopusService,
    IEnergyAggregationService aggregationService,
    IWeatherService weatherService,
    IWeatherAggregationService weatherAggregationService) : ControllerBase
{
    /// <summary>Returns raw half-hourly consumption readings for a date range.</summary>
    [HttpPost("consumption")]
    public async Task<ActionResult<IReadOnlyList<EnergyReading>>> GetConsumption(
        [FromBody] ConsumptionRequest request,
        CancellationToken cancellationToken)
    {
        var readings = await FetchReadingsAsync(request.OctopusApiKey, request, request.From, request.To, groupBy: null, cancellationToken);
        return Ok(readings);
    }

    /// <summary>Returns a side-by-side comparison of two user-selected periods.</summary>
    [HttpPost("comparison")]
    public async Task<ActionResult<PeriodComparisonResult>> GetComparison(
        [FromBody] ComparisonRequest request,
        CancellationToken cancellationToken)
    {
        var (fromA, toA) = GetPeriodBounds(request.Period, request.PeriodAStart);
        var (fromB, toB) = GetPeriodBounds(request.Period, request.PeriodBStart);

        var groupBy = request.Period switch
        {
            ComparisonPeriod.Year => "month",
            _ => "day"
        };

        var requestA = new ConsumptionRequest(
            request.OctopusApiKey, request.ElectricityMpan, request.ElectricitySerial,
            request.GasMprn, request.GasSerial, fromA, toA, request.FuelType);
        var requestB = new ConsumptionRequest(
            request.OctopusApiKey, request.ElectricityMpan, request.ElectricitySerial,
            request.GasMprn, request.GasSerial, fromB, toB, request.FuelType);

        var taskA = FetchReadingsAsync(request.OctopusApiKey, requestA, fromA, toA, groupBy, cancellationToken);
        var taskB = FetchReadingsAsync(request.OctopusApiKey, requestB, fromB, toB, groupBy, cancellationToken);
        await Task.WhenAll(taskA, taskB);

        var result = aggregationService.BuildPeriodComparison(
            taskA.Result, taskB.Result, request.Period, request.PeriodAStart, request.PeriodBStart);

        return Ok(result);
    }

    /// <summary>Returns correlation data points (energy vs weather) for scatter plot visualisation.</summary>
    [HttpPost("correlation")]
    public async Task<ActionResult<IReadOnlyList<CorrelationPoint>>> GetCorrelation(
        [FromBody] CorrelationRequest request,
        CancellationToken cancellationToken)
    {
        var consumptionRequest = new ConsumptionRequest(
            request.OctopusApiKey,
            request.ElectricityMpan,
            request.ElectricitySerial,
            request.GasMprn,
            request.GasSerial,
            request.From,
            request.To,
            request.FuelType);

        // Use daily grouping for correlations — one data point per day is sufficient for scatter plots.
        var energyTask = FetchReadingsAsync(request.OctopusApiKey, consumptionRequest, request.From, request.To, groupBy: "day", cancellationToken);
        var weatherTask = weatherService.GetHistoricalWeatherAsync(
            request.Latitude,
            request.Longitude,
            DateOnly.FromDateTime(request.From.Date),
            DateOnly.FromDateTime(request.To.Date),
            cancellationToken);

        await Task.WhenAll(energyTask, weatherTask);

        var fuelTypes = request.FuelType.HasValue
            ? [request.FuelType.Value]
            : new[] { FuelType.Electricity, FuelType.Gas };

        var points = fuelTypes
            .SelectMany(ft => weatherAggregationService.BuildCorrelationPoints(
                energyTask.Result, weatherTask.Result, ft))
            .ToList();

        return Ok(points);
    }

    private async Task<IReadOnlyList<EnergyReading>> FetchReadingsAsync(
        string apiKey,
        ConsumptionRequest request,
        DateTimeOffset from,
        DateTimeOffset to,
        string? groupBy = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task<IReadOnlyList<EnergyReading>>>();

        if (!string.IsNullOrWhiteSpace(request.ElectricityMpan) && !string.IsNullOrWhiteSpace(request.ElectricitySerial))
        {
            tasks.Add(octopusService.GetConsumptionAsync(
                apiKey,
                new MeterConfiguration(request.ElectricityMpan, request.ElectricitySerial),
                FuelType.Electricity,
                from, to, groupBy, cancellationToken));
        }

        if (!string.IsNullOrWhiteSpace(request.GasMprn) && !string.IsNullOrWhiteSpace(request.GasSerial))
        {
            tasks.Add(octopusService.GetConsumptionAsync(
                apiKey,
                new MeterConfiguration(request.GasMprn, request.GasSerial),
                FuelType.Gas,
                from, to, groupBy, cancellationToken));
        }

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).ToList();
    }

    private static (DateTimeOffset From, DateTimeOffset To) GetPeriodBounds(ComparisonPeriod period, DateTimeOffset start)
    {
        var to = period switch
        {
            ComparisonPeriod.Year  => start.AddYears(1),
            ComparisonPeriod.Month => start.AddMonths(1),
            ComparisonPeriod.Week  => start.AddDays(7),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
        return (start, to);
    }
}
