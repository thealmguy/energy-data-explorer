using System.Text.Json;
using EnergyDataExplorer.Api.Services;

namespace EnergyDataExplorer.Api.Services;

public sealed class GeocodeService(IHttpClientFactory httpClientFactory) : IGeocodeService
{
    private const string ClientName = "postcodes";
    private const string BaseUrl = "https://api.postcodes.io/postcodes";

    public async Task<(double Latitude, double Longitude)> ResolvePostcodeAsync(
        string postcode,
        CancellationToken cancellationToken = default)
    {
        var encodedPostcode = Uri.EscapeDataString(postcode.Trim());
        var client = httpClientFactory.CreateClient(ClientName);
        using var response = await client.GetAsync($"{BaseUrl}/{encodedPostcode}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PostcodesIoResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Unexpected null response from postcodes.io.");

        return (result.Result.Latitude, result.Result.Longitude);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private sealed record PostcodesIoResponse(int Status, PostcodesIoResult Result);

    private sealed record PostcodesIoResult(double Latitude, double Longitude);
}
