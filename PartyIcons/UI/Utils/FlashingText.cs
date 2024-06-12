using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace PartyIcons.UI.Utils;

public class FlashingText
{
    public FlashingText()
    {
        _flashColor0 = new Vector4(0.4f, 0.1f, 0.1f, 1.0f);
        _flashColor1 = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
    }

    public bool IsFlashing
    {
        get => _isFlashing;
        set
        {
            if (_isFlashing != value)
            {
                _isFlashing = value;

                if (value)
                {
                    _stopwatch.Start();
                }
                else
                {
                    _stopwatch.Stop();
                }
            }
        }
    }

    public void Draw(Action draw)
    {
        _ = Draw(() =>
        {
            draw.Invoke();
            return true;
        });
    }
    
    public bool Draw(Func<bool> draw)
    {
        Vector4 flashColor = _flashColor0;

        if (IsFlashing)
        {
            if (_stopwatch.ElapsedMilliseconds < FlashIntervalMs)
            {
                flashColor = _flashColor1;
            }

            if (_stopwatch.ElapsedMilliseconds > FlashIntervalMs * 2)
            {
                _stopwatch.Restart();
            }
        }


        bool result;
        using (ImRaii.PushColor(ImGuiCol.Text, flashColor, IsFlashing)) {
            result = draw.Invoke();
        }

        return result;
    }
    
    private readonly Vector4 _flashColor0;
    private readonly Vector4 _flashColor1;

    private static readonly Stopwatch _stopwatch = new();
    private const int FlashIntervalMs = 500;
    private bool _isFlashing;
}