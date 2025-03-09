using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using BepInEx.Logging;
using static BanMod.Utils;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace BanMod;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class BanMod : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static string modVersion = "1.0.2";
    public static List<string> supportedAU = new List<string> { "2024.11.26" };
    public static bool hasAccess = true;
    public Coroutines coroutines;
    public static ConfigEntry<bool> UnlockFPS { get; private set; }
    public static ConfigEntry<bool> AutoStart { get; private set; }
    public static Dictionary<byte, PlayerVersion> PlayerVersion = [];
    public static ManualLogSource Logger;
    public static ConfigEntry<string> WebhookUrl { get; private set; }
    public static bool CheckBanPlayer;
    public static ConfigEntry<string> HideName { get; private set; }
    public static ConfigEntry<string> HideColor { get; private set; }
    public const string ModColor = "#00ffff";
    public static readonly Dictionary<int, int> SayStartTimes = [];
    public static readonly Dictionary<int, int> SayBanwordsTimes = [];
    public static readonly List<(string Message, byte ReceiverID, string Title)> MessagesToSend = [];
    public static readonly Dictionary<byte, string> AllPlayerNames = [];
    public static BanMod Instance;

    public static bool isChatCommand = false;
    public static Dictionary<byte, PlayerState> PlayerStates = [];
    public static bool IntroDestroyed;
    public static int UpdateTime;
    public static PlayerControl[] AllPlayerControls
    {
        get
        {
            int count = PlayerControl.AllPlayerControls.Count;
            var result = new PlayerControl[count];
            int i = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.PlayerId == 255) continue;
                result[i++] = pc;
            }

            if (i == 0) return [];

            Array.Resize(ref result, i);
            return result;
        }
    }
    public static Color UnityModColor
    {
        get
        {
            if (!_unityModColor.HasValue)
            {
                if (ColorUtility.TryParseHtmlString(ModColor, out var unityColor))
                {
                    _unityModColor = unityColor;
                }
                else
                {
                    // failure
                    return Color.gray;
                }
            }
            return _unityModColor.Value;
        }
    }
    private static Color? _unityModColor;
    public static PlayerControl[] AllAlivePlayerControls
    {
        get
        {
            int count = PlayerControl.AllPlayerControls.Count;
            var result = new PlayerControl[count];
            int i = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.PlayerId == 255 || pc.Data.Disconnected ) continue;
                result[i++] = pc;
            }

            if (i == 0) return [];

            Array.Resize(ref result, i);
            return result;
        }
    }

    public void StartCoroutine(IEnumerator coroutine)
    {
        if (coroutine == null)
        {
            return;
        }

        coroutines.StartCoroutine(coroutine.WrapToIl2Cpp());
    }
    public override void Load()
    {

        Translator.Init();
        SpamManager.Init();
        BanManager.Init();

        Harmony.PatchAll();
       
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu")
            {
                ModManager.Instance.ShowModStamp(); // Required by InnerSloth Modding Policy

            }
        }));
    }
}
public class Coroutines : MonoBehaviour
{
}
