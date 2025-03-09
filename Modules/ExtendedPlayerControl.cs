using System;
using System.Linq;
using InnerNet;
using Rewired.Utils.Platforms.Windows;
using UnityEngine;
using static BanMod.Utils;

namespace BanMod;

static class ExtendedPlayerControl
{

    
    public static ClientData GetClient(this PlayerControl player)
    {
        try
        {
            return AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Character.PlayerId == player.PlayerId);
        }
        catch
        {
            return null;
        }
    }
    public static int GetClientId(this PlayerControl player)
    {
        if (player == null) return -1;
        var client = player.GetClient();
        return client?.Id ?? -1;
    }

    public static bool IsAlive(this PlayerControl target)
    {
        //In lobby all is alive
        if (Utils.IsLobby && !Utils.isInGame)
        {
            return true;
        }
        //if target is null, it is not alive
        if (target == null)
        {
            return false;
        }

        //if the target status is alive
        return !BanMod.PlayerStates.TryGetValue(target.PlayerId, out var playerState) || !playerState.IsDead;
    }
    public static string GetRealName(this PlayerControl player, bool isMeeting = false)
    {
        try
        {
            var name = isMeeting ? player.Data.PlayerName : player.name;
            return name.RemoveHtmlTags();
        }
        catch (NullReferenceException nullReferenceException)
        {
            Logger.Error($"{nullReferenceException.Message} - player is null? {player == null}", "GetRealName");
            return string.Empty;
        }
        catch (Exception exception)
        {
            ThrowException(exception);
            return string.Empty;
        }
    }
}