using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Data;
using InnerNet;
using UnityEngine;


namespace BanMod
{

    [HarmonyPatch]
    public static class Options
    {
        static Task taskOptionsLoad;
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize)), HarmonyPostfix]
        public static void OptionsLoadStart()
        {
            Logger.Info("Options.Load Start", "Options");
            taskOptionsLoad = Task.Run(Load);
        }
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
        public static void WaitOptionsLoad()
        {
            taskOptionsLoad.Wait();
            Logger.Info("Options.Load End", "Options");
        }





        public static OptionItem ApplyDenyNameList;
        public static OptionItem AddBanToList;
        public static OptionItem CheckBanList;
        public static OptionItem CheckBlockList;
        public static bool avoidBans = true;

        public static OptionItem AutoKickStart;
        public static OptionItem AutoKickStartAsBan;
        public static IntegerOptionItem AutoKickStartTimes;

        public static OptionItem AutoKickStopWords;
        public static OptionItem AutoKickStopWordsAsBan;
        public static IntegerOptionItem AutoKickStopWordsTimes;

        public static OptionItem AktiveLobby;
        public static OptionItem DisableLobbyMusic;
        public static bool ForceOwnLanguage = true;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;

            AktiveLobby = BooleanOptionItem.Create(1_000_097, "AktiveLobby", true, TabGroup.Setting, true);
            DisableLobbyMusic = BooleanOptionItem.Create(1_000_098, "DisableLobbyMusic", true, TabGroup.Setting, true);

            ApplyDenyNameList = BooleanOptionItem.Create(1_000_100, "ApplyDenyNameList", false, TabGroup.Banlist, true);
            AddBanToList = BooleanOptionItem.Create(1_000_101, "AddBanToList", false, TabGroup.Banlist, true);
            CheckBanList = BooleanOptionItem.Create(1_000_102, "CheckBanList", false, TabGroup.Banlist, true);
            CheckBlockList = BooleanOptionItem.Create(1_000_103, "CheckBlockList", false, TabGroup.Banlist, true);

            AutoKickStart = BooleanOptionItem.Create(1_000_104, "AutoKickStart", true, TabGroup.Spamlist, true);
            AutoKickStartAsBan = BooleanOptionItem.Create(1_000_105, "AutoKickStartAsBan", false, TabGroup.Spamlist, true);
            AutoKickStartTimes = (IntegerOptionItem)IntegerOptionItem.Create(1_000_106, "AutoKickStartTimes", new(1, 3, 1), 2, TabGroup.Spamlist,false)
                .SetValueFormat(OptionFormat.Times);

            AutoKickStopWords = BooleanOptionItem.Create(1_000_107, "AutoKickStopWords", true, TabGroup.Wordlist, true);
            AutoKickStopWordsAsBan = BooleanOptionItem.Create(1_000_108, "AutoKickStopWordsAsBan", false, TabGroup.Wordlist, true);
            AutoKickStopWordsTimes = (IntegerOptionItem)IntegerOptionItem.Create(1_000_109, "AutoKickStopWordsTimes", new(1, 3, 1), 2, TabGroup.Wordlist, false)
                .SetValueFormat(OptionFormat.Times);


            IsLoaded = true;
        }

    }
}