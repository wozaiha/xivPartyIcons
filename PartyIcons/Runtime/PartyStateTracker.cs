using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;

namespace PartyIcons.Runtime;

public sealed class PartyStateTracker : IDisposable
{
    private readonly ulong[] _hudState = new ulong[8];
    private readonly PartySlot[] _partyState = new PartySlot[8];
    private short _lastPartySize;

    public bool InParty => _lastPartySize > 1;
    public short PartySize => _lastPartySize;

    public event Action<PartyChangeType>? OnPartyStateChange;

    public void Enable()
    {
        Service.Framework.Update += FrameworkOnUpdate;
    }

    public void Dispose()
    {
        Service.Framework.Update -= FrameworkOnUpdate;
    }

    private unsafe void FrameworkOnUpdate(IFramework framework)
    {
        var agentHud = AgentHUD.Instance();

        var partySize = agentHud->PartyMemberCount;
        if (partySize == 0 || (partySize == 1 && _lastPartySize == 1)) {
            return;
        }

        var gm = GroupManager.Instance();
        if (gm == null) return;

        var change = PartyChangeType.None;

        for (var i = 0; i < 8; i++) {
            var contentId = agentHud->PartyMemberListSpan[i].ContentId;
            if (contentId > 0 && _hudState[i] != contentId) {
                _hudState[i] = contentId;
                change = PartyChangeType.Order;
            }
        }

        if (partySize != _lastPartySize) {
            change = PartyChangeType.Member;
        }

        for (var i = 0; i < partySize; i++) {
            var member = gm->PartyMembersSpan.GetPointer(i);
            if (member == null) continue;

            var slot = _partyState[i];

            if (member->ContentID != slot.ContentId) {
                change = PartyChangeType.Member;
                _partyState[i] = new PartySlot(contentId: member->ContentID, job: member->ClassJob);
            }
            else if (member->ClassJob != slot.Job) {
                // Skip change notification for job changing to 0 (member moved out of range, which may be temporary)
                if (member->ClassJob != 0) {
                    change = change < PartyChangeType.Job ? PartyChangeType.Job : change;
                }
                _partyState[i] = new PartySlot(contentId: member->ContentID, job: member->ClassJob);
            }
        }

        _lastPartySize = partySize;

        if (change != PartyChangeType.None) {
            Service.Log.Debug($"OnPartyChange ({change})");
            OnPartyStateChange?.Invoke(change);
        }
    }
}

internal struct PartySlot(long contentId, byte job)
{
    public readonly long ContentId = contentId;
    public readonly byte Job = job;
}

public enum PartyChangeType : byte
{
    None,
    Order,
    Job,
    Member
}