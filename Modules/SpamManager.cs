using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using static BanMod.Translator;
using static BanMod.ChatCommands;
using AmongUs.Data;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using UnityEngine;
using static InnerNet.ClientData;
using static ChatController;
using static FilterPopUp.FilterInfoUI;
namespace BanMod;

public static class SpamManager
{

    private static readonly string BANEDWORDS_FILE_PATH = "./BAN_DATA/BanWords.txt";
    internal static string msg1;
    internal static string msg2;
    public static List<string> BanWords = [];

    

    public static void Init()
    {
        CreateIfNotExists();
        BanWords = ReturnAllNewLinesInFile(BANEDWORDS_FILE_PATH);
    }
    public static void CreateIfNotExists()
    {
        if (!File.Exists(BANEDWORDS_FILE_PATH))
        {
            try
            {
                if (!Directory.Exists("BAN_DATA")) Directory.CreateDirectory("BAN_DATA");
                if (File.Exists("./BanWords.txt")) File.Move("./BanWords.txt", BANEDWORDS_FILE_PATH);
                else
                {
                    string fileName;
                    string[] name = CultureInfo.CurrentCulture.Name.Split("-");
                    if (name.Length >= 2)
                        fileName = name[0] switch
                        {
                            "zh" => "SChinese",
                            "ru" => "Russian",
                            "it" => "Italian",
                            _ => "English"
                        };
                    else fileName = "English";
                    Logger.Warn($"创建新的 BanWords 文件：{fileName}", "SpamManager");
                    File.WriteAllText(BANEDWORDS_FILE_PATH, GetResourcesTxt($"BanMod.Resources.BanWords.{fileName}.txt"));
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "SpamManager");
            }
        }
    }
    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public static List<string> ReturnAllNewLinesInFile(string filename)
    {
        if (!File.Exists(filename)) return [];
        using StreamReader sr = new(filename, Encoding.GetEncoding("UTF-8"));
        string text;

        List<string> sendList = [];
        while ((text = sr.ReadLine()) != null)
            if (text.Length > 1 && text != "") sendList.Add(text);
        return sendList;
    }

public static bool CheckSpam(PlayerControl player, string text)
    {
        if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId || BanMod.ExcludeFriends.Value && IsPlayerFriends(player.FriendCode)) return false;
        string name = player.GetRealName();
        bool kick = false;
        string msg = string.Empty;
        if (Options.AutoKickStart.GetBool())
        {
            if (ContainsStart(text) && Utils.IsLobby)
            {
                if (!BanMod.SayStartTimes.ContainsKey(player.GetClientId())) BanMod.SayStartTimes.Add(player.GetClientId(), 0);
                BanMod.SayStartTimes[player.GetClientId()]++;
                HudManager.Instance.Notifier.AddDisconnectMessage(string.Format(name + GetString("SayStart")));
                msg1 = string.Format(GetString("Warning") + name + GetString("SpamWarning") + BanMod.SayStartTimes[player.GetClientId()] + " / " + Options.AutoKickStartTimes.GetInt() + ")");
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, name + GetString("SayStart") + GetString("WarningSpam"));
                if (BanMod.SayStartTimes[player.GetClientId()] > Options.AutoKickStartTimes.GetInt())
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage(string.Format(name + GetString("KickSayStart")));
                    kick = true;
                }

                if (kick) AmongUsClient.Instance.KickPlayer(player.GetClientId(), Options.AutoKickStartAsBan.GetBool());
                return true;
            }
        }

        bool banned = BanWords.Any(text.Contains);
        if (!banned) return false;

        if (Options.AutoKickStopWords.GetBool())
        {
            if (!BanMod.SayBanwordsTimes.ContainsKey(player.GetClientId())) BanMod.SayBanwordsTimes.Add(player.GetClientId(), 0);
            BanMod.SayBanwordsTimes[player.GetClientId()]++;
            HudManager.Instance.Notifier.AddDisconnectMessage(string.Format(name + GetString("SayBanWord")));
            msg2 = string.Format(GetString("Warning") + name + GetString("WordWarning") + BanMod.SayBanwordsTimes[player.GetClientId()] + " / " + Options.AutoKickStopWordsTimes.GetInt() + ")");
            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, name + GetString("SayBanWord") + GetString("WarningWord"));
            if (BanMod.SayBanwordsTimes[player.GetClientId()] > Options.AutoKickStopWordsTimes.GetInt())
            {
                HudManager.Instance.Notifier.AddDisconnectMessage(string.Format(name + GetString("KickSayBanWord")));
                kick = true;
            }
        }

        if (kick) AmongUsClient.Instance.KickPlayer(player.GetClientId(), Options.AutoKickStopWordsAsBan.GetBool());
        return true;
    }

    private static bool ContainsStart(string text)
    {
        text = text.Trim().ToLower();

        int stNum = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i..].Equals("k")) stNum++;
            if (text[i..].Equals("开")) stNum++;
        }
        if (stNum >= 3) return true;

        if (text == Translator.GetString("1")) return true;
        if (text == "start") return true;
        if (text == "/Start") return true;
        if (text == "/Start/") return true;
        if (text == "Start/") return true;
        if (text == "/start") return true;
        if (text == "/start/") return true;
        if (text == "start/") return true;
        if (text == "plsstart") return true;
        if (text == "pls start") return true;
        if (text == "please start") return true;
        if (text == "pleasestart") return true;
        if (text == "Plsstart") return true;
        if (text == "Pls start") return true;
        if (text == "Please start") return true;
        if (text == "Pleasestart") return true;
        if (text == "plsStart") return true;
        if (text == "pls Start") return true;
        if (text == "please Start") return true;
        if (text == "pleaseStart") return true;
        if (text == "PlsStart") return true;
        if (text == "Pls Start") return true;
        if (text == "Please Start") return true;
        if (text == "PleaseStart") return true;
        if (text == "sTart") return true;
        if (text == "stArt") return true;
        if (text == "staRt") return true;
        if (text == "starT") return true;
        if (text == "s t a r t") return true;
        if (text == "S t a r t") return true;
        if (text == "started") return true;
        if (text == "Started") return true;
        if (text == "s t a r t e d") return true;
        if (text == "S t a r t e d") return true;
        if (text == "Го") return true;
        if (text == "гО") return true;
        if (text == "го") return true;
        if (text == "Гоу") return true;
        if (text == "гоу") return true;
        if (text == "Старт") return true;
        if (text == "старт") return true;
        if (text == "/Старт") return true;
        if (text == "/Старт/") return true;
        if (text == "Старт/") return true;
        if (text == "/старт") return true;
        if (text == "/старт/") return true;
        if (text == "старт/") return true;
        if (text == "пжстарт") return true;
        if (text == "пж старт") return true;
        if (text == "пжСтарт") return true;
        if (text == "пж Старт") return true;
        if (text == "Пжстарт") return true;
        if (text == "Пж старт") return true;
        if (text == "ПжСтарт") return true;
        if (text == "Пж Старт") return true;
        if (text == "сТарт") return true;
        if (text == "стАрт") return true;
        if (text == "стаРт") return true;
        if (text == "старТ") return true;
        if (text == "с т а р т") return true;
        if (text == "С т а р т") return true;
        if (text == "начни") return true;
        if (text == "Начни") return true;
        if (text == "начинай") return true;
        if (text == "начинай уже") return true;
        if (text == "Начинай") return true;
        if (text == "Начинай уже") return true;
        if (text == "Начинай Уже") return true;
        if (text == "н а ч и н а й") return true;
        if (text == "Н а ч и н а й") return true;
        if (text == "пж го") return true;
        if (text == "пжго") return true;
        if (text == "Пж Го") return true;
        if (text == "Пж го") return true;
        if (text == "пж Го") return true;
        if (text == "ПжГо") return true;
        if (text == "Пжго") return true;
        if (text == "пжГо") return true;
        if (text == "ГоПж") return true;
        if (text == "гоПж") return true;
        if (text == "Гопж") return true;
        if (text == "开") return true;
        if (text == "快开") return true;
        if (text == "开始") return true;
        if (text == "开啊") return true;
        if (text == "开阿") return true;
        if (text == "kai") return true;
        if (text == "kaishi") return true;
        if (text == "vai") return true;
        if (text == "Vai") return true;
        if (text == "Inizia") return true;
        if (text == "Iniziamo") return true;
        if (text == "inizia") return true;
        if (text == "iniziamo") return true;
        if (text == "Startala") return true;
        if (text == "premi start") return true;
        if (text == "parti") return true;
        if (text == "falla partire") return true;
        if (text.Contains("Inizia")) return true;
        if (text.Contains("inizia")) return true;
        if (text.Contains("Sono pronto")) return true;
        if (text.Contains("avvia")) return true;
        if (text.Contains("avviala")) return true;
        if (text.Contains("startala")) return true;
        if (text.Contains("metti pronto")) return true;
        if (text.Contains("premi pronto")) return true;
        if (text.Contains("premi start")) return true;
        if (text.Contains("premi inizia")) return true;
        if (text.Contains("iniziala")) return true;
        if (text.Contains("iniziaaaaaa")) return true;
        if (text.Contains("st4rt")) return true;
        if (text.Contains("strata")) return true;
        if (text.Contains("start")) return true;
        if (text.Contains("Start")) return true;
        if (text.Contains("STart")) return true;
        if (text.Contains("s t a r t")) return true;
        if (text.Contains("begin")) return true;
        if (text.Contains('了')) return false;
        if (text.Contains('没')) return false;
        if (text.Contains('吗')) return false;
        if (text.Contains('哈')) return false;
        if (text.Contains('还')) return false;
        if (text.Contains('现')) return false;
        if (text.Contains('不')) return false;
        if (text.Contains('可')) return false;
        if (text.Contains('刚')) return false;
        if (text.Contains('的')) return false;
        if (text.Contains('打')) return false;
        if (text.Contains('门')) return false;
        if (text.Contains('关')) return false;
        if (text.Contains('怎')) return false;
        if (text.Contains('要')) return false;
        if (text.Contains('摆')) return false;
        if (text.Contains('啦')) return false;
        if (text.Contains('咯')) return false;
        if (text.Contains('嘞')) return false;
        if (text.Contains('勒')) return false;
        if (text.Contains('心')) return false;
        if (text.Contains('呢')) return false;
        if (text.Contains('门')) return false;
        if (text.Contains('总')) return false;
        if (text.Contains('哥')) return false;
        if (text.Contains('姐')) return false;
        if (text.Contains('《')) return false;
        if (text.Contains('?')) return false;


        return text.Contains('开') || text.Contains("kai");
    }
    
}

