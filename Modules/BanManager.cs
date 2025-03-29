using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using InnerNet;

namespace BanMod;

public static class BanManager
{
    private const string DenyNameListPath = "./BAN_DATA/DenyName.txt";
    private const string BanListPath = "./BAN_DATA/BanList.txt";

    public static void Init()
    {
        try
        {
            Directory.CreateDirectory("BAN_DATA");

            if (!File.Exists(BanListPath))
            {
                Logger.Warn("Create a new BanList.txt file", "BanManager");
                File.Create(BanListPath).Close();
            }

            if (!File.Exists(DenyNameListPath))
            {
                Logger.Warn("Create a new DenyName.txt file", "BanManager");
                File.Create(DenyNameListPath).Close();
            }

        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "BanManager");
        }
    }
    public static bool CheckDenyNamePlayer(PlayerControl player, string name)
    {
        if (!AmongUsClient.Instance.AmHost || !Options.ApplyDenyNameList.GetBool()) return false;

        try
        {
            Directory.CreateDirectory("BAN_DATA");
            if (!File.Exists(DenyNameListPath)) File.Create(DenyNameListPath).Close();
            using StreamReader sr = new(DenyNameListPath);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "") continue;
                if (line.Contains("Amogus"))
                {
                    AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
                    Logger.Info($"{name}?????{line}?????????????????", "Kick");
                    return true;
                }
                if (line.Contains("Amogus V"))
                {
                    AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
                    Logger.Info($"{name}?????{line}?????????????????", "Kick");
                    return true;
                }

                if (Regex.IsMatch(name, line))
                {
                    AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
                    Logger.Info($"{name}?????{line}?????????????????", "Kick");
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "CheckDenyNamePlayer");
            return true;
        }
    }

    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string GetHashedPuid(this ClientData player)
    {
        if (player == null) return string.Empty;
        string puid = player.ProductUserId;
        using SHA256 sha256 = SHA256.Create();
        // get sha-256 hash
        byte[] sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(puid));
        string sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();

        // pick front 5 and last 4
        return string.Concat(sha256Hash.AsSpan(0, 5), sha256Hash.AsSpan(sha256Hash.Length - 4));
    }

    public static void AddBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || !BanMod.AddBanToList.Value || player == null) return;
        if (!CheckBanList(player.FriendCode, player.GetHashedPuid()))
        {
            if (player.GetHashedPuid() != "" && player.GetHashedPuid() != null && player.GetHashedPuid() != "e3b0cb855")
            {
                File.AppendAllText(BanListPath, $"{player.FriendCode},{player.GetHashedPuid()},{player.PlayerName.RemoveHtmlTags()}\n");
                HudManager.Instance.Notifier.AddDisconnectMessage(string.Format((player.PlayerName) + Translator.GetString("PlayerinBanList")));

            }
            else HudManager.Instance.Notifier.AddDisconnectMessage(string.Format((player.PlayerName) + Translator.GetString("PlayerNotinBanList")));
        }
    }


    public static void CheckBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || !Options.CheckBanList.GetBool()) return;

        string friendcode = player?.FriendCode;
        if (friendcode?.Length < 7) // #1234 is 5 chars, and it's impossible for a friend code to only have 3
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            HudManager.Instance.Notifier.AddDisconnectMessage(string.Format((player.PlayerName) + Translator.GetString("PlayerCodeInvalid")));
            return;
        }

        if (friendcode?.Count(c => c == '#') != 1)
        {
            // This is part of eac, so that's why it will say banned by EAC list.
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            HudManager.Instance.Notifier.AddDisconnectMessage(string.Format((player.PlayerName) + Translator.GetString("PlayerCodeInvalid")));
            return;
        }

        // Contains any non-word character or digits
        const string pattern = @"[\W\d]";
        if (Regex.IsMatch(friendcode[..friendcode.IndexOf("#", StringComparison.Ordinal)], pattern))
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            HudManager.Instance.Notifier.AddDisconnectMessage(string.Format((player.PlayerName) + Translator.GetString("PlayerCodeInvalid")));
            return;
        }

        if (CheckBanList(player?.FriendCode, player?.GetHashedPuid()))
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            HudManager.Instance.Notifier.AddDisconnectMessage(string.Format((player.PlayerName) + Translator.GetString("PlayerIsInBanList")));
            return;
        }

    }

    public static bool CheckBanList(string code, string hashedpuid = "")
    {
        bool OnlyCheckPuid = false;
        if (code == "" && hashedpuid != "") OnlyCheckPuid = true;
        else if (code == "") return false;
        try
        {
            Directory.CreateDirectory("BAN_DATA");
            if (!File.Exists(BanListPath)) File.Create(BanListPath).Close();
            using StreamReader sr = new(BanListPath);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "") continue;
                if (!OnlyCheckPuid)
                    if (line.Contains(code))
                        return true;
                if (line.Contains(hashedpuid)) return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "CheckBanList");
        }

        return false;
    }

}


[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.Select))]
class BanMenuSelectPatch
{
    public static void Postfix(BanMenu __instance, int clientId)
    {
        ClientData recentClient = AmongUsClient.Instance.GetRecentClient(clientId);
        if (recentClient == null) return;
        if (!BanManager.CheckBanList(recentClient.FriendCode, recentClient.GetHashedPuid()))
            __instance.BanButton.GetComponent<ButtonRolloverHandler>().SetEnabledColors();

    }
}