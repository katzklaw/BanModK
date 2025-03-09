using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using System.Threading.Tasks;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;

namespace BanMod;


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        var rpcType = (RpcCalls)callId;
        MessageReader subReader = MessageReader.Get(reader);
        switch (rpcType)
        {
            
            case RpcCalls.SendChat: // Free chat
                var text = subReader.ReadString();
                ChatCommands.OnReceiveChat(__instance, text, out var canceled);
                if (canceled) return false;
                break;
            case RpcCalls.SendQuickChat:
                ChatCommands.OnReceiveChat(__instance, "Some message from quick chat", out var canceledQuickChat);
                if (canceledQuickChat) return false;
                break;
            
        }
        if (
            ((!Enum.IsDefined(typeof(RpcCalls), callId)))) 
        {
            if (AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), false);
            }
            return false;
        }
        return true;
    }
    
}