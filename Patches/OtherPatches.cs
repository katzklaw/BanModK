using HarmonyLib;
using AmongUs.Data;
using UnityEngine;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Rewired.Utils.Platforms.Windows;
using Hazel;
using InnerNet;
using TMPro;

namespace BanMod;





[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTracker_Update
{
    // Postfix patch of PingTracker.Update to show mod name & ping
    public static void Postfix(PingTracker __instance)
    {
        __instance.text.alignment = TMPro.TextAlignmentOptions.Center;

        if (AmongUsClient.Instance.IsGameStarted){

            __instance.aspectPosition.DistanceFromEdge = new Vector3(-0.21f, 0.50f, 0f);

            __instance.text.text = $"{Utils.getColoredPingText(AmongUsClient.Instance.Ping)}";
            
            return;
        }

        __instance.text.text = $"{Utils.getColoredPingText(AmongUsClient.Instance.Ping)}";
        
    }
}

[HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
public static class FreeChatInputField_UpdateCharCount
{
    // Postfix patch of FreeChatInputField.UpdateCharCount to change how charCountText displays
    public static void Postfix(FreeChatInputField __instance)
    {
 
        int length = __instance.textArea.text.Length;
        __instance.charCountText.SetText($"{length}/{__instance.textArea.characterLimit}");

        if (length < 85)
        { // Under 75%

            __instance.charCountText.color = UnityEngine.Color.black;

        }
        else if (length < 100)
        { // Under 100%

            __instance.charCountText.color = UnityEngine.Color.yellow;

        }
        else if (length < 120)
        { // Under 100%

            __instance.charCountText.color = UnityEngine.Color.red;

        }
    }
}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShower_Start
{
    private static void Postfix(VersionShower __instance)
    {
        BanMod.credentialsText = $"<size=70%><size=85%><color={BanMod.ModColor}>{BanMod.ModName}</color> v{BanMod.PluginDisplayVersion}</size>";
        BanMod.credentialsText += $"\r\n<color=#a54aff>By <color=#f34c50>Gianni</color>";

        var credentials = Object.Instantiate(__instance.text);
        credentials.text = BanMod.credentialsText;
        credentials.alignment = TextAlignmentOptions.Right;
        credentials.transform.position = new Vector3(1f, 2.67f, -2f);
        credentials.fontSize = credentials.fontSizeMax = credentials.fontSizeMin = 2f;

    }
}



[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update
{

    public static void Postfix(HudManager __instance)
    {
        __instance.ShadowQuad.gameObject.SetActive(!Utils.fullBrightActive()); // Fullbright

        if (Utils.chatUiActive())
        { // AlwaysChat
            __instance.Chat.gameObject.SetActive(true);
        }
        else
        {
            Utils.closeChat();
            __instance.Chat.gameObject.SetActive(false);
        }
    }
}


[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
public static class AmongUsClient_Update
{
    public static void Postfix()
    {
        Spoof.spoofLevel();

    }
}

