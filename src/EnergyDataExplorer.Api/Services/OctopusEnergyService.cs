using System.Net.Http.Headers;
using System.Text.Json;
using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Services;

public sealed class OctopusEnergyService(IHttpClientFactory httpClientFactory) : IOctopusEnergyService
{
    private const string ClientName = "octopus";
    private const string BaseUrl = "https://api.octopus.energy/v1";

    public async Task<IReadOnlyList<EnergyReading>> GetConsumptionAsync(
        string apiKey,
        MeterConfiguration meter,
        FuelType fuelType,
        DateTimeOffset from,
        DateTimeOffset to,
        string? groupBy = null,
        CancellationToken cancellationToken = default)
    {
        var segment = fuelType == FuelType.Electricity
            ? $"electricity-meter-points/{meter.MeterPointReference}/meters/{meter.SerialNumber}/consumption"
            : $"gas-meter-points/{meter.MeterPointReference}/meters/{meter.SerialNumber}/consumption";

        var readings = new List<EnergyReading>();
        var nextUrl = BuildUrl(segment, from, to, groupBy);

        var client = httpClientFactory.CreateClient(ClientName);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{apiKey}:")));

        while (nextUrl is not null)
        {
            using var response = await client.GetAsync(nextUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var page = JsonSerializer.Deserialize<OctopusConsumptionPage>(json, JsonOptions);

            if (page?.Results is not null)
            {
                readings.AddRange(page.Results.Select(r => new EnergyReading(
                    r.IntervalStart,
                    r.IntervalEnd,
                    r.Consumption,
                    fuelType)));
            }

            nextUrl = page?.Next;
        }

        return readings;
    }

    private static string BuildUrl(string segment, DateTimeOffset from, DateTimeOffset to, string? groupBy) =>
        $"{BaseUrl}/{segment}/?period_from={Uri.EscapeDataString(from.ToString("o"))}" +
        $"&period_to={Uri.EscapeDataString(to.ToString("o"))}&page_size=1500&order_by=period" +
        (groupBy is not null ? $"&group_by={groupBy}" : string.Empty);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private sealed record OctopusConsumptionPage(
        int Count,
        string? Next,
        string? Previous,
        IReadOnlyList<OctopusConsumptionResult>? Results);

    private sealed record OctopusConsumptionResult(
        decimal Consumption,
        DateTimeOffset IntervalStart,
        DateTimeOffset IntervalEnd);
}
