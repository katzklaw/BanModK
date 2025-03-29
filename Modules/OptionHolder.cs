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
        public static OptionItem CheckBanList;
        public static OptionItem CheckBlockList;
        public static bool avoidBans = true;

        public static OptionItem AutoKickStart;
        public static OptionItem AutoKickStartAsBan;
        public static IntegerOptionItem AutoKickStartTimes;

        public static OptionItem AutoKickStopWords;
        public static OptionItem AutoKickStopWordsAsBan;
        public static IntegerOptionItem AutoKickStopWordsTimes;

        public static bool ForceOwnLanguage = true;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;

            ApplyDenyNameList = BooleanOptionItem.Create(1_000_100, "ApplyDenyNameList", true, TabGroup.Banlist, true);
            CheckBanList = BooleanOptionItem.Create(1_000_102, "CheckBanList", true, TabGroup.Banlist, true);
            CheckBlockList = BooleanOptionItem.Create(1_000_103, "CheckBlockList", true, TabGroup.Banlist, true);

            AutoKickStart = BooleanOptionItem.Create(1_000_104, "AutoKickStart", true, TabGroup.Spamlist, true);
            AutoKickStartAsBan = BooleanOptionItem.Create(1_000_105, "AutoKickStartAsBan", false, TabGroup.Spamlist, true);
            AutoKickStartTimes = (IntegerOptionItem)IntegerOptionItem.Create(1_000_106, "AutoKickStartTimes", new(0, 5, 1), 2, TabGroup.Spamlist,false)
                .SetValueFormat(OptionFormat.Times);

            AutoKickStopWords = BooleanOptionItem.Create(1_000_109, "AutoKickStopWords", true, TabGroup.Wordlist, true);
            AutoKickStopWordsAsBan = BooleanOptionItem.Create(1_000_110, "AutoKickStopWordsAsBan", false, TabGroup.Wordlist, true);
            AutoKickStopWordsTimes = (IntegerOptionItem)IntegerOptionItem.Create(1_000_111, "AutoKickStopWordsTimes", new(0, 5, 1), 1, TabGroup.Wordlist, false)
                .SetValueFormat(OptionFormat.Times);

            IsLoaded = true;
        }

    }
}