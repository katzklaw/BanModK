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
using System.Linq;

namespace BanMod;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class BanMod : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static string modVersion = "1.2.1";
    public static List<string> supportedAU = new List<string> { "2025.3.25" };
    public static readonly string ModName = "BanMod";
    public const string PluginDisplayVersion = "1.2.1";
    public static bool hasAccess = true;
    public Coroutines coroutines;
    public static ConfigEntry<bool> AktiveChat { get; private set; }
    public static ConfigEntry<bool> ExcludeFriends { get; private set; }
    public static ConfigEntry<string> spoofLevel { get; private set; }
    public static ConfigEntry<string> FriendCode { get; private set; }
    public static ConfigEntry<bool> AktiveLobby { get; private set; }
    public static ConfigEntry<bool> DisableLobbyMusic { get; private set; }
    public static ConfigEntry<bool> AddBanToList { get; private set; }
    public static ConfigEntry<bool> EnableZoom { get; private set; }
    public static bool IsChatCommand;
    public static string credentialsText;
    public static OptionBackupData RealOptionsData;
    public static Dictionary<byte, PlayerVersion> PlayerVersion = [];
    public static readonly Dictionary<byte, Color32> PlayerColors = [];
    public static Dictionary<int, PlayerVersion> playerVersion = [];
    public static ConfigEntry<int> MessageWait { get; private set; }
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

    public static PlayerControl[] AllPlayerControls => PlayerControl.AllPlayerControls.ToArray().Where(p => p != null).ToArray();
    public static PlayerControl[] AllAlivePlayerControls => PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && p.IsAlive() ).ToArray();

    public static bool isChatCommand = false;
    public static Dictionary<byte, PlayerState> PlayerStates = [];
    public static bool IntroDestroyed;
    public static int UpdateTime;
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
        Instance = this;


        AktiveChat = Config.Bind("Client Options", "AktiveChat", false);
        AktiveLobby = Config.Bind("Client Options", "AktiveLobby", false);
        DisableLobbyMusic = Config.Bind("Client Options", "DisableLobbyMusic", false);
        AddBanToList = Config.Bind("Client Options", "AddBanToList", false);
        EnableZoom = Config.Bind("Client Options", "EnableZoom", false);
        ExcludeFriends = Config.Bind("Client Options", "ExcludeFriends", false);
        spoofLevel = Config.Bind("Client Options", "Level","");
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
