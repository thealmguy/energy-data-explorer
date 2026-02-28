using EnergyDataExplorer.Shared.Models;

namespace EnergyDataExplorer.Shared.State;

/// <summary>
/// Singleton in-memory state holding user-provided settings for the current session.
/// Data is never persisted; the user must re-enter settings on each app start.
/// </summary>
public sealed class AppSettingsState
{
    private AppSettings _settings = new();

    public AppSettings Current => _settings;

    public bool IsConfigured => _settings.IsConfigured;

    public event Action? OnChange;

    public void Update(AppSettings settings)
    {
        _settings = settings;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
