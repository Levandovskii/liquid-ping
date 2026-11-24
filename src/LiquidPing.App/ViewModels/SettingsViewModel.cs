using CommunityToolkit.Mvvm.Input;
using LiquidPing.App.Services;
using LiquidPing.Core.Models;
using LiquidPing.Core.Storage;

namespace LiquidPing.App.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsStore m_store;
    private readonly ThemeService m_theme;
    private readonly LocalizationService m_localization;
    private AppSettings m_settings = new();

    public SettingsViewModel(
        ISettingsStore store,
        ThemeService theme,
        LocalizationService localization)
    {
        m_store = store ?? throw new ArgumentNullException(nameof(store));
        m_theme = theme ?? throw new ArgumentNullException(nameof(theme));
        m_localization = localization ?? throw new ArgumentNullException(nameof(localization));
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public IAsyncRelayCommand SaveCommand { get; }

    public AppSettings Settings
    {
        get => m_settings;
        set => SetProperty(ref m_settings, value);
    }

    public async Task LoadAsync()
    {
        Settings = await m_store.LoadAsync(CancellationToken.None);
        m_theme.Apply(Settings.Theme);
        m_localization.Apply(Settings.Language);
    }

    public void ApplyTheme(ThemeMode theme)
    {
        Settings.Theme = theme;
        m_theme.Apply(theme);
    }

    private async Task SaveAsync()
    {
        await m_store.SaveAsync(Settings, CancellationToken.None);
    }
}
