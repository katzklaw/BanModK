using HarmonyLib;
using InnerNet;
using System;

namespace BanMod;


[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
static class OnPlayerJoinedPatch
{
    public static bool IsDisconnected(this ClientData client)
    {
        var __instance = AmongUsClient.Instance;
        for (int i = 0; i < __instance.allClients.Count; i++)
        {
            ClientData clientData = __instance.allClients[i];
            if (clientData.Id == client.Id)
            {
                return true;
            }
        }
        return false;
    }
    static bool IsPlayerFriend(this ClientData client)
    {
        var __instance = FriendsListManager.Instance;
        {
            if (IsPlayerFriend == FriendsListManager.Instance.IsPlayerFriend)
            {
                return true;
            }
        }

        return false;
    }
    public static void Postfix( /*AmongUsClient __instance,*/ [HarmonyArgument(0)] ClientData client)
    {
        Logger.Info($"{client.PlayerName} (ClientID: {client.Id} / FriendCode: {client.FriendCode} / Hashed PUID: {client.GetHashedPuid()}) joined the lobby", "Session");

        LateTask.New(() =>
        {
            try
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (!client.IsDisconnected() && client.Character.Data.IsIncomplete)
                    {
                        AmongUsClient.Instance.KickPlayer(client.Id, false);
                        HudManager.Instance.Notifier.AddDisconnectMessage(string.Format((client.PlayerName) + Translator.GetString("NotSpammed")));
                        return;
                    }

                }
            }
            catch
            {
            }
        }, 3f, "green bean kick late task", false);



        if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost && Options.CheckBlockList.GetBool())
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            HudManager.Instance.Notifier.AddDisconnectMessage(string.Format(client.PlayerName + Translator.GetString("Blocked")));
        }

        BanManager.CheckBanPlayer(client);
    }

}