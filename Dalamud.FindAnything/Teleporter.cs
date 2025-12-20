using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.STD;
using System;

namespace Dalamud.FindAnything;

internal enum TeleportResult
{
    Success,
    BadState,
    BadDestination,
}

public class Teleporter(IDalamudPluginInterface pluginInterface)
{
    private readonly IpcTeleporter ipcTeleporter = new(pluginInterface);

    public void Teleport(IAetheryteEntry entry) {
        try {
            var (result, showSuccessMessage) = ipcTeleporter.Teleport(entry);
            ShowTeleportResult(entry, result, showSuccessMessage);
            return;
        } catch (IpcNotReadyError) {
            FindAnythingPlugin.Log.Verbose("\"Teleporter\" IPC not found. Using built-in teleport.");
        }

        ShowTeleportResult(entry, BasicTeleporter.Teleport(entry), true);
    }

    private static void ShowTeleportResult(IAetheryteEntry entry, TeleportResult result, bool showSuccessMessage) {
        if (result != TeleportResult.Success) {
            FindAnythingPlugin.UserError(result switch {
                TeleportResult.BadState => "Cannot teleport in this situation.",
                TeleportResult.BadDestination => "Cannot teleport to that destination.",
                _ => "Teleport failed.",
            });
        } else if (showSuccessMessage) {
            FindAnythingPlugin.ChatGui.Print($"Teleported to {FindAnythingPlugin.AetheryteManager.GetAetheryteName(entry)}!");
        }
    }
}

internal class IpcTeleporter
{
    private readonly ICallGateSubscriber<uint, byte, bool> teleportIpc;
    private readonly ICallGateSubscriber<bool> showTeleportChatMessageIpc;

    public IpcTeleporter(IDalamudPluginInterface pluginInterface) {
        teleportIpc = pluginInterface.GetIpcSubscriber<uint, byte, bool>("Teleport");
        showTeleportChatMessageIpc = pluginInterface.GetIpcSubscriber<bool>("Teleport.ChatMessage");
    }

    public (TeleportResult, bool) Teleport(IAetheryteEntry entry) {
        if (teleportIpc.InvokeFunc(entry.AetheryteId, entry.SubIndex)) {
            return (TeleportResult.Success, showTeleportChatMessageIpc.InvokeFunc());
        }

        return (TeleportResult.BadState, false);
    }
}

internal static class BasicTeleporter
{
    private static unsafe bool UpdateList() {
        if (Control.GetLocalPlayer() == null)
            return false;

        try {
            var tp = Telepo.Instance();
            return tp->UpdateAetheryteList() != null;
        } catch (Exception ex) {
            FindAnythingPlugin.Log.Error(ex, "Error while updating the Aetheryte list for built-in teleport");
            return false;
        }
    }

    private static unsafe StdVector<TeleportInfo> GetList() {
        return Telepo.Instance()->TeleportList;
    }

    public static unsafe TeleportResult Teleport(IAetheryteEntry entry) {
        if (!UpdateList())
            return TeleportResult.BadState;

        if (!GetList().Exists(tp => tp.AetheryteId == entry.AetheryteId && tp.SubIndex == entry.SubIndex))
            return TeleportResult.BadDestination;

        if (ActionManager.Instance()->GetActionStatus(ActionType.Action, 5) != 0)
            return TeleportResult.BadState;

        if (Telepo.Instance()->Teleport(entry.AetheryteId, entry.SubIndex)) {
            return TeleportResult.Success;
        }

        return TeleportResult.BadState;
    }
}