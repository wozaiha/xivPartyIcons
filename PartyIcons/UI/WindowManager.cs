using Dalamud.Interface.Windowing;
using System;

namespace PartyIcons.UI;

public sealed class WindowManager : IDisposable
{
    private readonly SettingsWindow _settingsWindow;
    private readonly WindowSystem _windowSystem;

    public WindowManager()
    {
        _settingsWindow = new SettingsWindow();

        _windowSystem = new WindowSystem("PartyIcons");
        _windowSystem.AddWindow(_settingsWindow);

        Service.PluginInterface.UiBuilder.OpenConfigUi += _settingsWindow.Toggle;
        Service.PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
    }

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.OpenConfigUi -= _settingsWindow.Toggle;
        Service.PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
    }

    public void ToggleSettings()
    {
        _settingsWindow.Toggle();
    }
}