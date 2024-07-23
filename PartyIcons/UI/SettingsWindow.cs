using System;
using System.Numerics;
using ImGuiNET;
using PartyIcons.UI.Utils;
using PartyIcons.Utils;

namespace PartyIcons.UI;

public sealed class SettingsWindow : IDisposable
{
    public bool SettingsVisible
    {
        get => _settingsVisible;
        set
        {
            _settingsVisible = value;

            if (value)
            {
                _windowSizeHelper.ForceSize();
            }
        }
    }

    public void Initialize()
    {
        Service.PluginInterface.UiBuilder.Draw += DrawSettingsWindow;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OpenSettingsWindow;
        
        _generalSettings.Initialize();
        _nameplateSettings.Initialize();
    }

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.Draw -= DrawSettingsWindow;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= OpenSettingsWindow;
    }

    public void OpenSettingsWindow()
    {
        SettingsVisible = true;
    }
    
    public void ToggleSettingsWindow()
    {
        SettingsVisible = !SettingsVisible;
    }
    
    public void DrawSettingsWindow()
    {
        if (!SettingsVisible)
        {
            return;
        }

        _windowSizeHelper.SetWindowSize();

        if (ImGui.Begin("PartyIcons", ref _settingsVisible))
        {
            _windowSizeHelper.CheckWindowSize();

            if (!Plugin.Settings.SelectorsDialogComplete || UpgradeGuideSettings.ForceRedisplay) {
                UpgradeGuideSettings.Draw();
            }
            else if (ImGui.BeginTabBar("##tabbar"))
            {
                _generalTabText.IsFlashing = Plugin.Settings.TestingMode;
                
                if (_generalTabText.Draw(() => ImGui.BeginTabItem("General##general")))
                {
                    if (ImGui.BeginChild("##general_content"))
                    {
                        _generalSettings.DrawGeneralSettings();
                        
                        ImGui.EndChild();
                    }
                    
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Nameplates"))
                {
                    if (ImGui.BeginChild("##nameplates_content"))
                    {
                        _nameplateSettings.Draw();

                        ImGui.EndChild();
                    }

                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Appearance"))
                {
                    if (ImGui.BeginChild("##appearance_content"))
                    {
                        _appearanceSettings.Draw();

                        ImGui.EndChild();
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Status Icons"))
                {
                    if (ImGui.BeginChild("##statuses_content"))
                    {
                        StatusSettings.Draw();

                        ImGui.EndChild();
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Chat Names"))
                {
                    if (ImGui.BeginChild("##chat_names_content"))
                    {
                        _chatNameSettings.DrawChatNameSettings();
                        
                        ImGui.EndChild();
                    }
                    
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Roles##static_assignments"))
                {
                    if (ImGui.BeginChild("##static_assignments_content"))
                    {
                        _staticAssignmentsSettings.DrawStaticAssignmentsSettings();
                        
                        ImGui.EndChild();
                    }
                    
                    ImGui.EndTabItem();
                }
                
                ImGui.EndTabBar();
            }
        }

        ImGui.End();
    }

    public static void ImGuiHelpTooltip(string tooltip, bool experimental = false)
    {
        ImGui.SameLine();

        if (experimental)
        {
            ImGui.TextColored(new Vector4(0.8f, 0.0f, 0.0f, 1f), "!");
        }
        else
        {
            ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1f), "?");
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }
    }
    
    private bool _settingsVisible = false;
    private static WindowSizeHelper _windowSizeHelper = new();
    private readonly GeneralSettings _generalSettings = new();
    private readonly NameplateSettings _nameplateSettings = new();
    private readonly AppearanceSettings _appearanceSettings = new();
    private readonly ChatNameSettings _chatNameSettings = new();
    private readonly StaticAssignmentsSettings _staticAssignmentsSettings = new();

    private FlashingText _generalTabText = new();
}
