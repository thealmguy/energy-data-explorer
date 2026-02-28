namespace EnergyDataExplorer.Api.Models;

public sealed record WeatherRequest(
    double Latitude,
    double Longitude,
    DateOnly From,
    DateOnly To
);
