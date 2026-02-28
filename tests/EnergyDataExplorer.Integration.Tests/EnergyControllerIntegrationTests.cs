using System.Net;
using System.Net.Http.Json;
using EnergyDataExplorer.Api.Controllers;
using EnergyDataExplorer.Api.Models;
using EnergyDataExplorer.Shared.Enums;
using EnergyDataExplorer.Shared.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EnergyDataExplorer.Integration.Tests;

public sealed class EnergyControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EnergyControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostComparison_Returns200_WithMockedServices()
    {
        var mockOctopus = new Mock<EnergyDataExplorer.Api.Services.IOctopusEnergyService>();
        mockOctopus
            .Setup(s => s.GetConsumptionAsync(
                It.IsAny<string>(),
                It.IsAny<MeterConfiguration>(),
                It.IsAny<FuelType>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EnergyReading>
            {
                new(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, 1.5m, FuelType.Electricity)
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => mockOctopus.Object);
            });
        }).CreateClient();

        var periodA = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var periodB = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var request = new ComparisonRequest(
            "test-key", "mpan", "serial", "", "",
            ComparisonPeriod.Year, periodA, periodB, FuelType.Electricity);

        var response = await client.PostAsJsonAsync("/api/energy/comparison", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
