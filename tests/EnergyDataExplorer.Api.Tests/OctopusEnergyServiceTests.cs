using System.Net;
using System.Net.Http.Json;
using EnergyDataExplorer.Api.Services;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Api.Tests;

public sealed class OctopusEnergyServiceTests
{
    [Fact]
    public async Task GetConsumptionAsync_ReturnsReadings_WhenApiSucceeds()
    {
        var responseJson = """
            {
              "count": 2,
              "next": null,
              "previous": null,
              "results": [
                {
                  "consumption": 0.5,
                  "interval_start": "2024-01-01T00:00:00Z",
                  "interval_end": "2024-01-01T00:30:00Z"
                },
                {
                  "consumption": 0.6,
                  "interval_start": "2024-01-01T00:30:00Z",
                  "interval_end": "2024-01-01T01:00:00Z"
                }
              ]
            }
            """;

        var factory = CreateFactory(HttpStatusCode.OK, responseJson);
        var sut = new OctopusEnergyService(factory);

        var result = await sut.GetConsumptionAsync(
            "test-key",
            new MeterConfiguration("1234567890", "ABC123"),
            FuelType.Electricity,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        Assert.Equal(2, result.Count);
        Assert.Equal(0.5m, result[0].ConsumptionKwh);
        Assert.Equal(FuelType.Electricity, result[0].FuelType);
    }

    [Fact]
    public async Task GetConsumptionAsync_Throws_WhenApiReturnsError()
    {
        var factory = CreateFactory(HttpStatusCode.Unauthorized, "{}");
        var sut = new OctopusEnergyService(factory);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.GetConsumptionAsync(
                "bad-key",
                new MeterConfiguration("123", "ABC"),
                FuelType.Gas,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow));
    }

    private static IHttpClientFactory CreateFactory(HttpStatusCode status, string json)
    {
        var handler = new MockHttpMessageHandler(status, json);
        var client = new HttpClient(handler);
        var factory = new MockHttpClientFactory(client);
        return factory;
    }
}

file sealed class MockHttpMessageHandler(HttpStatusCode status, string responseBody) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(status)
        {
            Content = new StringContent(responseBody, System.Text.Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}

file sealed class MockHttpClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}
