using EnergyDataExplorer.Web.Services;
using EnergyDataExplorer.Shared.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EnergyDataExplorer.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddSingleton<AppSettingsState>();
builder.Services.AddScoped<IEnergyApiClient, EnergyApiClient>();

var host = builder.Build();

// Pre-populate settings from server configuration (user secrets / appsettings).
// Silently ignored if the API is unavailable or nothing is configured.
var apiClient = host.Services.GetRequiredService<IEnergyApiClient>();
var settingsState = host.Services.GetRequiredService<AppSettingsState>();
var serverSettings = await apiClient.GetServerSettingsAsync();
if (serverSettings is { IsFromServerConfig: true })
    settingsState.Update(serverSettings);

await host.RunAsync();
