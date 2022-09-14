﻿using System;
using System.Linq;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;
using PartyIcons.Stylesheet;
using PartyIcons.View;

namespace PartyIcons.Runtime;

public sealed class ChatNameUpdater : IDisposable
{
    [PluginService]
    private ClientState ClientState { get; set; }

    [PluginService]
    private PartyList PartyList { get; set; }

    [PluginService]
    private ObjectTable ObjectTable { get; set; }

    [PluginService]
    private ChatGui ChatGui { get; set; }

    private readonly RoleTracker _roleTracker;
    private readonly PlayerStylesheet _stylesheet;

    public ChatConfig PartyMode { get; set; }
    public ChatConfig OthersMode { get; set; }

    public ChatNameUpdater(RoleTracker roleTracker, PlayerStylesheet stylesheet)
    {
        _roleTracker = roleTracker;
        _stylesheet = stylesheet;
    }

    public void Enable()
    {
        ChatGui.ChatMessage += OnChatMessage;
    }

    private void OnChatMessage(XivChatType type, uint senderid, ref SeString sender, ref SeString message,
        ref bool ishandled)
    {
        if (ClientState.IsPvP)
        {
            return;
        }

        if (type == XivChatType.Say || type == XivChatType.Party || type == XivChatType.Alliance ||
            type == XivChatType.Shout || type == XivChatType.Yell)
        {
            Parse(type, ref sender);
        }
    }

    public void Disable()
    {
        ChatGui.ChatMessage -= OnChatMessage;
    }

    public void Dispose()
    {
        Disable();
    }

    private PlayerPayload GetPlayerPayload(SeString sender)
    {
        var playerPayload = sender.Payloads.FirstOrDefault(p => p is PlayerPayload) as PlayerPayload;

        if (playerPayload == null)
        {
            playerPayload = new PlayerPayload(ClientState.LocalPlayer.Name.TextValue,
                ClientState.LocalPlayer.HomeWorld.Id);
        }

        return playerPayload;
    }

    private bool CheckIfPlayerPayloadInParty(PlayerPayload playerPayload)
    {
        foreach (var member in PartyList)
        {
            if (member.Name.ToString() == playerPayload.PlayerName && member.World.Id == playerPayload.World.RowId)
            {
                return true;
            }
        }

        return false;
    }

    private bool GetAndRemovePartyNumberPrefix(XivChatType type, SeString sender, out string prefix)
    {
        if (type == XivChatType.Party || type == XivChatType.Alliance)
        {
            var playerNamePayload = sender.Payloads.FirstOrDefault(p => p is TextPayload) as TextPayload;
            prefix = playerNamePayload.Text.Substring(0, 1);
            playerNamePayload.Text = playerNamePayload.Text.Substring(1);

            return true;
        }
        else
        {
            prefix = "";

            return false;
        }
    }

    private void RemoveExistingForeground(SeString str)
    {
        str.Payloads.RemoveAll(p => p.Type == PayloadType.UIForeground);
    }

    private ClassJob? FindSenderJob(PlayerPayload playerPayload)
    {
        ClassJob? senderJob = null;

        foreach (var member in PartyList)
        {
            if (member.Name.ToString() == playerPayload.PlayerName && member.World.Id == playerPayload.World.RowId)
            {
                senderJob = member.ClassJob.GameData;

                break;
            }
        }

        if (senderJob == null)
        {
            foreach (var obj in ObjectTable)
            {
                if (obj is PlayerCharacter pc && pc.Name.ToString() == playerPayload.PlayerName &&
                    pc.HomeWorld.Id == playerPayload.World.RowId)
                {
                    senderJob = pc.ClassJob.GameData;

                    break;
                }
            }
        }

        return senderJob;
    }

    private void Parse(XivChatType chatType, ref SeString sender)
    {
        var playerPayload = GetPlayerPayload(sender);

        var config = CheckIfPlayerPayloadInParty(playerPayload) ? PartyMode : OthersMode;

        if (config.Mode == ChatMode.Role &&
            _roleTracker.TryGetAssignedRole(playerPayload.PlayerName, playerPayload.World.RowId, out var roleId))
        {
            GetAndRemovePartyNumberPrefix(chatType, sender, out _);

            var prefixString = new SeString();
            if (config.Colored)
            {
                RemoveExistingForeground(sender);
                prefixString.Append(new UIForegroundPayload(_stylesheet.GetRoleChatColor(roleId)));
            }
            prefixString.Append(_stylesheet.GetRoleChatPrefix(roleId));
            prefixString.Append(new TextPayload(" "));

            sender.Payloads.InsertRange(0, prefixString.Payloads);
            if (config.Colored) sender.Payloads.Add(UIForegroundPayload.UIForegroundOff);
        }
        else if (config.Mode != ChatMode.GameDefault || config.Colored) // still get in if GameDefault && Colored
        {
            var senderJob = FindSenderJob(playerPayload);

            if (senderJob == null || senderJob.RowId == 0)
            {
                return;
            }

            GetAndRemovePartyNumberPrefix(chatType, sender, out var numberPrefix);

            var prefixString = new SeString();

            switch (config.Mode)
            {
                case ChatMode.Job:
                    if (config.Colored)
                    {
                        RemoveExistingForeground(sender);
                        prefixString.Append(new UIForegroundPayload(_stylesheet.GetJobChatColor(senderJob)));
                    }

                    if (numberPrefix.Length > 0)
                    {
                        prefixString.Append(new TextPayload(numberPrefix));
                    }

                    prefixString.Append(_stylesheet.GetJobChatPrefix(senderJob, config.Colored).Payloads);
                    prefixString.Append(new TextPayload(" "));

                    break;

                case ChatMode.Role:
                    if (config.Colored)
                    {
                        RemoveExistingForeground(sender);
                        prefixString.Append(new UIForegroundPayload(_stylesheet.GetGenericRoleChatColor(senderJob)));
                    }

                    if (numberPrefix.Length > 0)
                    {
                        prefixString.Append(new TextPayload(numberPrefix));
                    }

                    prefixString.Append(_stylesheet.GetGenericRoleChatPrefix(senderJob, config.Colored).Payloads);
                    prefixString.Append(new TextPayload(" "));

                    break;

                case ChatMode.GameDefault:
                    // don't need to check Colored again
                    RemoveExistingForeground(sender);
                    prefixString.Append(new UIForegroundPayload(_stylesheet.GetGenericRoleChatColor(senderJob)));

                    if (numberPrefix.Length > 0)
                    {
                        prefixString.Append(new TextPayload(numberPrefix));
                    }

                    prefixString.Append(new TextPayload(" "));

                    break;

                default:
                    throw new ArgumentException();
            }

            sender.Payloads.InsertRange(0, prefixString.Payloads);
            if (config.Colored) sender.Payloads.Add(UIForegroundPayload.UIForegroundOff);
        }
    }
}
