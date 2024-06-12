using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace PartyIcons.Utils;

public static class ImGuiExt
{
    public static bool ButtonEnabledWhen(bool enabled, string text)
    {
        if (!enabled)
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
        var result = ImGui.Button(text);
        if (!enabled)
            ImGui.PopStyleVar();

        return result && enabled;
    }

    public static void HoverTooltip(string text)
    {
        if (!ImGui.IsItemHovered()) {
            return;
        }

        ImGui.BeginTooltip();
        ImGui.TextUnformatted(text);
        ImGui.EndTooltip();
    }

    public static void Spacer(int size)
    {
        ImGui.Dummy(new Vector2(0, size));
    }

    public static void SectionHeader(string text)
    {
        ImGui.Dummy(new Vector2(0, 2f));
        ImGui.PushStyleColor(0, ImGuiHelpers.DefaultColorPalette()[0]);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 2f));
    }

    public static void SetComboWidth(IEnumerable<string> values)
    {
        const float paddingMultiplier = 1.05f;
        float maxItemWidth = float.MinValue;

        foreach (var text in values)
        {
            var itemWidth = ImGui.CalcTextSize(text).X + ImGui.GetStyle().ScrollbarSize * 3f;
            maxItemWidth = Math.Max(maxItemWidth, itemWidth);
        }

        ImGui.SetNextItemWidth(maxItemWidth * paddingMultiplier);
    }
}