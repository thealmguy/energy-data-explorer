using EnergyDataExplorer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
        }
    });
});

builder.Services.AddHttpClient("octopus");
builder.Services.AddHttpClient("openmeteo");
builder.Services.AddHttpClient("postcodes");

builder.Services.AddScoped<IOctopusEnergyService, OctopusEnergyService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IGeocodeService, GeocodeService>();
builder.Services.AddScoped<IEnergyAggregationService, EnergyAggregationService>();
builder.Services.AddScoped<IWeatherAggregationService, WeatherAggregationService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose Program class for integration testing with WebApplicationFactory
public partial class Program { }
