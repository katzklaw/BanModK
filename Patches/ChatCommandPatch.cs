using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using Mono.Cecil.Mdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static BanMod.SpamManager;
using static BanMod.Translator;
using static InnerNet.ClientData;
using System.Reflection;
using System.Text;
using System.Globalization;
using Rewired.Utils.Platforms.Windows;

namespace BanMod;


[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal class ChatCommands
{


    public static List<string> ChatHistory = [];

    public static bool Prefix(ChatController __instance)
    {

        if (!AmongUsClient.Instance.AmHost) return true;
        var text = __instance.freeChatField.textArea.text;
        if (ChatHistory.Count == 0 || ChatHistory[^1] != text) ChatHistory.Add(text);
        if (__instance.timeSinceLastMessage < 3f) return false;
        ChatControllerUpdatePatch.CurrentHistorySelection = ChatHistory.Count;
        string[] args = text.Split(' ');
        string subArgs = "";
        var canceled = false;
        BanMod.isChatCommand = true;
        Logger.Info(text, "SendChat");
        {
            BanMod.isChatCommand = true;
            switch (args[0])
            {

                //case "/id":
                //    canceled = true;
                //    string msgText1 = GetString("PlayerIdList");
                //    foreach (var pc in BanMod.AllPlayerControls)
                //    {

                //        if (pc == null) continue;
                //        msgText1 += "\n" + pc.PlayerId.ToString() + "→" + pc.GetRealName();
                //    }
                //    HudManager.Instance.Notifier.AddDisconnectMessage(msgText1);
                //    canceled = true;
                //    break;
                case "/id":
                    canceled = true;
                    string msgText = GetString("PlayerIdList");
                    foreach (var pc in BanMod.AllPlayerControls)
                    {

                        if (pc == null) continue;
                        msgText += "\n" + pc.PlayerId.ToString() + "→" + pc.GetRealName();
                    }
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msgText);
                    canceled = true;
                    break;

                case "/addfriends":
                case "/addf":
                case "/add":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    if (string.IsNullOrEmpty(subArgs) || !byte.TryParse(subArgs, out byte newFRIENDSId))
                    {
                        break;
                    }
                    var newFRIENDS = Utils.GetPlayerById(newFRIENDSId);
                    var FRIENDSADDED = newFRIENDS.FriendCode;
                    string name = newFRIENDS.GetRealName();
                    if (newFRIENDS == null)
                    {
                        break;
                    }
                    if (IsPlayerFriends(FRIENDSADDED))
                    {
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, name + GetString("PlayerinFriendList"));
                    }
                    else
                    {
                        File.AppendAllText("./BAN_DATA/Friends.txt", $"\n{FRIENDSADDED}");
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, name + GetString("AddedtoFriendList"));
                    }
                    break;

                case "/deletefriends":
                case "/deletef":
                case "/dlt":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    if (string.IsNullOrEmpty(subArgs) || !byte.TryParse(subArgs, out byte FRIENDSId))
                    {
                        break;
                    }
                    var FRIENDS = Utils.GetPlayerById(FRIENDSId);
                    var FRIENDSDELETED = FRIENDS.FriendCode;
                    string name1 = FRIENDS.GetRealName();
                    if (FRIENDS == null)
                    {
                        break;
                    }
                    if (!IsPlayerFriends(FRIENDSDELETED))
                    {
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, name1 + GetString("PlayerNotFriends"));
                    }
                    else
                    {
                        var lines = File.ReadAllLines("./BAN_DATA/Friends.txt").Where(line => !line.Contains(FRIENDSDELETED)).ToArray();
                        File.WriteAllLines("./BAN_DATA/Friends.txt", lines);
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, name1 + GetString("PlayerRemovedFromFriendsList"));
                    }

                    break;

                case "/level":
                    canceled = true;
                    subArgs = args.Length < 2 ? "" : args[1];
                    _ = int.TryParse(subArgs, out int input);
                    if (input is < 1 or > 999)
                    {
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, GetString("Message.AllowLevelRange"));
                        break;
                    }
                    var number = Convert.ToUInt32(input);
                    PlayerControl.LocalPlayer.RpcSetLevel(number - 1);
                    DataManager.Player.stats.level = number - 1;
                    DataManager.Player.Save();
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, GetString("Message.SetLevel") + subArgs);
                    break;

                case "/spam":
                    canceled = true;
                    if (__instance.timeSinceLastMessage < 3.0f) return false;
                    else PlayerControl.LocalPlayer.RpcSendChat(msg1);
                    break;

                case "/word":
                    canceled = true;
                    if (__instance.timeSinceLastMessage < 3.0f) return false;
                    else PlayerControl.LocalPlayer.RpcSendChat(msg2);
                    break;

                case "/msg":
                    canceled = true;
                    if (__instance.timeSinceLastMessage < 3.0f) return false;
                    else PlayerControl.LocalPlayer.RpcSendChat(string.Format(GetString("msg")));
                    break;

                case "/msgs":
                    canceled = true;
                    if (__instance.timeSinceLastMessage < 3.0f) return false;
                    else PlayerControl.LocalPlayer.RpcSendChat(string.Format(GetString("msgs")));
                    break;

                case "/msgw":
                    canceled = true;
                    if (__instance.timeSinceLastMessage < 3.0f) return false;
                    else PlayerControl.LocalPlayer.RpcSendChat(string.Format(GetString("msgw")));
                    break;


                case "/aiuto":
                case "/help":
                case "/h":
                    canceled = true;
                    Utils.ShowHelp(PlayerControl.LocalPlayer.PlayerId);
                    break;

                case "/chiudi":
                    canceled = true;
                    List<MeetingHud.VoterState> statesList = [];
                    MeetingHud.Instance.RpcVotingComplete(statesList.ToArray(), null, true);
                    MeetingHud.Instance.Close();
                    MeetingHud.Instance.RpcClose();
                    break;
            }
        }
        goto Skip;
    Skip:
        return !canceled;
    }
    public static bool IsPlayerFriends(string friendCode)
    {
    if (friendCode == "" || friendCode == string.Empty || !BanMod.ExcludeFriends.Value) return false;

    const string friendCodesFilePath = "./BAN_DATA/Friends.txt";
    if (!File.Exists(friendCodesFilePath))
    {
        File.WriteAllText(friendCodesFilePath, string.Empty);
        return false;
    }

    var friendCodes = File.ReadAllLines(friendCodesFilePath);
    return friendCodes.Any(code => code.Contains(friendCode, StringComparison.OrdinalIgnoreCase));
    }


    public static void OnReceiveChat(PlayerControl player, string text, out bool canceled)
    {
        canceled = false;
        if (!AmongUsClient.Instance.AmHost) return;

        if (text.StartsWith("\n")) text = text[1..];
        string[] args = text.Split(' ');


        switch (args[0])
        {


            default:
                if (SpamManager.CheckSpam(player, text)) return; 
                break;
        }

    }

}