using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;

namespace PartyIcons.Utils;

public static class SeStringUtils
{
    public static SeString Text(string rawText)
    {
        var seString = new SeString(new List<Payload>());
        seString.Append(new TextPayload(rawText));

        return seString;
    }

    // Defaults to glowColor 51 (black glow) for maximum contrast
    public static SeString Text(string text, ushort fgColor, ushort glowColor = 51)
    {
        var seString = new SeString(new List<Payload>());
        seString.Append(new UIGlowPayload(glowColor));
        seString.Append(new UIForegroundPayload(fgColor));
        seString.Append(new TextPayload(text));
        seString.Append(UIForegroundPayload.UIForegroundOff);
        seString.Append(UIGlowPayload.UIGlowOff);

        return seString;
    }

    public static string PrintRawStringArg(IntPtr arg)
    {
        var seString = MemoryHelper.ReadSeStringNullTerminated(arg);
        return string.Join("", seString.Payloads.Select(payload => $"[{payload}]"));
    }
}
