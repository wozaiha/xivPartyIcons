using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using PartyIcons.UI.Utils;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Numerics;
using System.Text.RegularExpressions;

namespace PartyIcons.UI;

public sealed class GeneralSettings
{
    private readonly Notice _notice = new();

    public void Initialize()
    {
        _notice.Initialize();
    }

    private FlashingText _testingModeText = new();
    
    public void DrawGeneralSettings()
    {
        ImGui.Dummy(new Vector2(0, 2f));

        using (ImRaii.PushColor(ImGuiCol.CheckMark, 0xFF888888)) {
            var usePriorityIcons = true;
            ImGui.Checkbox("##usePriorityIcons", ref usePriorityIcons);
            ImGui.SameLine();
            ImGui.Text("Prioritize status icons");
            using (ImRaii.PushIndent())
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudOrange)) {
                ImGui.TextWrapped(
                    "Note: Priority status icons are now configured per nameplate type from the 'Appearance' tab via the 'Swap Style' option. You can also configure which icons are considered important enough to prioritize in the 'Status Icons' tab.");
            }
            ImGui.Dummy(new Vector2(0, 3));
        }

        var testingMode = Plugin.Settings.TestingMode;
        if (ImGui.Checkbox("##testingMode", ref testingMode))
        {
            Plugin.Settings.TestingMode = testingMode;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        _testingModeText.IsFlashing = Plugin.Settings.TestingMode;
        _testingModeText.Draw(() => ImGui.Text("Enable testing mode"));
        // ImGui.Text("Enable testing mode");
        ImGuiComponents.HelpMarker("Applies settings to any player, contrary to only the ones that are in the party.");

        var chatContentMessage = Plugin.Settings.ChatContentMessage;

        if (ImGui.Checkbox("##chatmessage", ref chatContentMessage))
        {
            Plugin.Settings.ChatContentMessage = chatContentMessage;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("Display chat message when entering duty");
        ImGuiComponents.HelpMarker("Can be used to determine the duty type before fully loading in.");

        ImGuiExt.Spacer(10);
        if (ImGuiExt.ButtonEnabledWhen(ImGui.GetIO().KeyCtrl, "Show upgrade guide again")) {
            UpgradeGuideSettings.ForceRedisplay = true;
        }
        ImGuiExt.HoverTooltip("Hold Control to allow clicking");

        _notice.DisplayNotice();
    }
}

public sealed class Notice
{
    public Notice()
    {
        _httpClient = new HttpClient();
    }

    private HttpClient _httpClient;
    
    public void Initialize()
    {
        DownloadAndParseNotice();
    }
        
    private string? _noticeString;
    private string? _noticeUrl;
    
    private void DownloadAndParseNotice()
    {
        try
        {
            var stringAsync = _httpClient.GetStringAsync("https://shdwp.github.io/ukraine/xiv_notice.txt");
            stringAsync.Wait();
            var strArray = stringAsync.Result.Split('|');

            if ((uint) strArray.Length > 0U)
            {
                _noticeString = Regex.Replace(strArray[0], "\n", "\n\n");
            }

            if (strArray.Length <= 1)
            {
                return;
            }

            _noticeUrl = strArray[1];

            if (!(_noticeUrl.StartsWith("http://") || _noticeUrl.StartsWith("https://")))
            {
                Service.Log.Warning($"Received invalid noticeUrl {_noticeUrl}, ignoring");
                _noticeUrl = null;
            }
        }
        catch (Exception ex) { }
    }

    public void DisplayNotice()
    {
        if (_noticeString == null)
        {
            return;
        }

        ImGui.Dummy(new Vector2(0.0f, 15f));
        ImGui.PushStyleColor((ImGuiCol) 0, ImGuiColors.DPSRed);
        ImGuiHelpers.SafeTextWrapped(_noticeString);

        if (_noticeUrl != null)
        {
            if (ImGui.Button(_noticeUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _noticeUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex) { }
            }
        }

        ImGui.PopStyleColor();
    }
}