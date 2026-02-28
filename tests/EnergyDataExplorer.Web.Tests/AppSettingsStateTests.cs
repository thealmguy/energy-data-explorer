using EnergyDataExplorer.Shared.Models;
using EnergyDataExplorer.Shared.State;

namespace EnergyDataExplorer.Web.Tests;

public sealed class AppSettingsStateTests
{
    [Fact]
    public void IsConfigured_ReturnsFalse_WhenEmpty()
    {
        var sut = new AppSettingsState();
        Assert.False(sut.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsTrue_WhenRequiredFieldsSet()
    {
        var sut = new AppSettingsState();
        sut.Update(new AppSettings
        {
            OctopusApiKey = "test-key",
            Postcode = "SW1A 1AA",
            ElectricityMeter = new MeterConfiguration("1234567890", "ABC123")
        });

        Assert.True(sut.IsConfigured);
    }

    [Fact]
    public void Update_FiresOnChangeEvent()
    {
        var sut = new AppSettingsState();
        var fired = false;
        sut.OnChange += () => fired = true;

        sut.Update(new AppSettings { OctopusApiKey = "k" });

        Assert.True(fired);
    }

    [Fact]
    public void Update_ReplacesCurrentSettings()
    {
        var sut = new AppSettingsState();
        sut.Update(new AppSettings { OctopusApiKey = "first" });
        sut.Update(new AppSettings { OctopusApiKey = "second" });

        Assert.Equal("second", sut.Current.OctopusApiKey);
    }
}
