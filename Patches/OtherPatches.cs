using HarmonyLib;
using AmongUs.Data;
using UnityEngine;

namespace BanMod;




[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShower_Start
{
    public static void Postfix(VersionShower __instance)
    {
        if (BanMod.supportedAU.Contains(Application.version)){ // Checks if Among Us version is supported

            __instance.text.text =  $"BanMod v{BanMod.modVersion} (v{Application.version})"; // Supported
        
        }else{

            __instance.text.text =  $"BanMod v{BanMod.modVersion} (<color=red>v{Application.version}</color>)"; //Unsupported
        
        }
    }
}
[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTracker_Update
{
    // Postfix patch of PingTracker.Update to show mod name & ping
    public static void Postfix(PingTracker __instance)
    {
        __instance.text.alignment = TMPro.TextAlignmentOptions.Center;

        if (AmongUsClient.Instance.IsGameStarted){

            __instance.aspectPosition.DistanceFromEdge = new Vector3(-0.21f, 0.50f, 0f);

            __instance.text.text = $"<color=#00FFFF>BanMod</color> by <color=#ffff00ff>Bart</color>         {Utils.getColoredPingText(AmongUsClient.Instance.Ping)}";
            
            return;
        }

        __instance.text.text = $"<color=#00FFFF>BanMod</color> by <color=#ffff00ff>Bart</color>         {Utils.getColoredPingText(AmongUsClient.Instance.Ping)}";
        
    }
}


[HarmonyPatch(typeof(StatsManager), nameof(StatsManager.BanMinutesLeft), MethodType.Getter)]
public static class StatsManager_BanMinutesLeft_Getter
{
    // Prefix patch of Getter method for StatsManager.BanMinutesLeft to remove disconnect penalty
    public static void Postfix(StatsManager __instance, ref int __result)
    {
        {
            __instance.BanPoints = 0f; // Removes all BanPoints
            __result = 0; // Removes all BanMinutes
        }
    }
}






