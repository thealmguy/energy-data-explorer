using System.Text.Json;
using System.Text.Json.Serialization;
using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Services;

public sealed class WeatherService(IHttpClientFactory httpClientFactory) : IWeatherService
{
    private const string ClientName = "openmeteo";
    private const string BaseUrl = "https://archive-api.open-meteo.com/v1/archive";

    public async Task<IReadOnlyList<WeatherReading>> GetHistoricalWeatherAsync(
        double latitude,
        double longitude,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}?latitude={latitude}&longitude={longitude}" +
                  $"&start_date={from:yyyy-MM-dd}&end_date={to:yyyy-MM-dd}" +
                  "&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m,sunshine_duration" +
                  "&wind_speed_unit=kmh&timezone=Europe%2FLondon";

        var client = httpClientFactory.CreateClient(ClientName);
        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<OpenMeteoResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Unexpected null response from open-meteo API.");

        return BuildReadings(data);
    }

    private static IReadOnlyList<WeatherReading> BuildReadings(OpenMeteoResponse data)
    {
        var times     = data.Hourly?.Time;
        var temps     = data.Hourly?.Temperature2M;
        var humidity  = data.Hourly?.RelativeHumidity2M;
        var wind      = data.Hourly?.WindSpeed10M;
        var sunshine  = data.Hourly?.SunshineDuration;

        if (times is null || temps is null || humidity is null || wind is null || sunshine is null)
            return [];

        var readings = new List<WeatherReading>(times.Count);
        for (var i = 0; i < times.Count; i++)
        {
            readings.Add(new WeatherReading(
                DateTimeOffset.Parse(times[i]),
                temps[i],
                humidity[i],
                wind[i],
                sunshine[i] / 60.0)); // API returns seconds; convert to minutes
        }
        return readings;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private sealed record OpenMeteoResponse(OpenMeteoHourly Hourly);

    private sealed record OpenMeteoHourly(
        IReadOnlyList<string> Time,
        [property: JsonPropertyName("temperature_2m")]        IReadOnlyList<double>? Temperature2M,
        [property: JsonPropertyName("relative_humidity_2m")] IReadOnlyList<double>? RelativeHumidity2M,
        [property: JsonPropertyName("wind_speed_10m")]        IReadOnlyList<double>? WindSpeed10M,
        [property: JsonPropertyName("sunshine_duration")]     IReadOnlyList<double>? SunshineDuration);
}
