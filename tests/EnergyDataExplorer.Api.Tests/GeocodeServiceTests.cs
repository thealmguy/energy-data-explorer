using System.Net;
using EnergyDataExplorer.Api.Services;

namespace EnergyDataExplorer.Api.Tests;

public sealed class GeocodeServiceTests
{
    [Fact]
    public async Task ResolvePostcodeAsync_ReturnsCoordinates_WhenApiSucceeds()
    {
        const string responseJson = """
            {
              "status": 200,
              "result": {
                "latitude": 51.5074,
                "longitude": -0.1278
              }
            }
            """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var client = new HttpClient(handler);
        var factory = new MockHttpClientFactory(client);
        var sut = new GeocodeService(factory);

        var (lat, lon) = await sut.ResolvePostcodeAsync("SW1A 1AA");

        Assert.Equal(51.5074, lat, precision: 4);
        Assert.Equal(-0.1278, lon, precision: 4);
    }

    [Fact]
    public async Task ResolvePostcodeAsync_Throws_WhenApiReturnsError()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "{}");
        var client = new HttpClient(handler);
        var factory = new MockHttpClientFactory(client);
        var sut = new GeocodeService(factory);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.ResolvePostcodeAsync("INVALID"));
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
