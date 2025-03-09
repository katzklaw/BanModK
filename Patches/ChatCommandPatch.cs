using HarmonyLib;
using System.Collections.Generic;


namespace BanMod;


[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal class ChatCommands
{
    
    private static readonly Dictionary<char, int> Pollvotes = [];
    private static readonly Dictionary<char, string> PollQuestions = [];
    private static readonly List<byte> PollVoted = [];

    public const string Csize = "85%"; // CustomRole Settings Font-Size
    public const string Asize = "75%"; // All Appended Addons Font-Size

    public static List<string> ChatHistory = [];

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
