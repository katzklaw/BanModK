using System;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;
using static BanMod.Translator;
using Object = UnityEngine.Object;

namespace BanMod
{
    [HarmonyPatch(typeof(GameSettingMenu))]
    public static class GameSettingMenuPatch
    {
        private static GameOptionsMenu SettingsTab;
        private static PassiveButton SettingsButton;
        public static CategoryHeaderMasked BanlistCategoryHeader { get; private set; }
        public static CategoryHeaderMasked SpamlistCategoryHeader { get; private set; }
        public static CategoryHeaderMasked WordlistCategoryHeader { get; private set; }
        public static FreeChatInputField InputField;

        [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPostfix]
        public static void StartPostfix(GameSettingMenu __instance)
        {
            SettingsTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
            SettingsTab.name = MenuName;
            var vanillaOptions = SettingsTab.GetComponentsInChildren<OptionBehaviour>();
            foreach (var vanillaOption in vanillaOptions)
            {
                Object.Destroy(vanillaOption.gameObject);
            }

            // TOH設定ボタンのスペースを作るため，左側の要素を上に詰める
            var gameSettingsLabel = __instance.transform.Find("GameSettingsLabel");
            if (gameSettingsLabel)
            {
                gameSettingsLabel.localPosition += Vector3.up * 0.2f;
            }
            __instance.MenuDescriptionText.transform.parent.localPosition += Vector3.up * 0.4f;
            __instance.GamePresetsButton.transform.parent.localPosition += Vector3.up * 0.5f;

            // TOH設定ボタン
            SettingsButton = Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);
            SettingsButton.name = "ModSettingsButton";
            SettingsButton.transform.localPosition = __instance.RoleSettingsButton.transform.localPosition + (__instance.RoleSettingsButton.transform.localPosition - __instance.GameSettingsButton.transform.localPosition);
            SettingsButton.buttonText.DestroyTranslator();
            SettingsButton.buttonText.text = GetString("ModSettingsLabel");
            var activeSprite = SettingsButton.activeSprites.GetComponent<SpriteRenderer>();
            var selectedSprite = SettingsButton.selectedSprites.GetComponent<SpriteRenderer>();
            activeSprite.color = selectedSprite.color = BanMod.UnityModColor;
            SettingsButton.OnClick.AddListener((Action)(() =>
            {
                __instance.ChangeTab(-1, false);  // バニラタブを閉じる
                SettingsTab.gameObject.SetActive(true);
                __instance.MenuDescriptionText.text = GetString("ModDescription");
                SettingsButton.SelectButton(true);
            }));

            // 各カテゴリの見出しを作成
            BanlistCategoryHeader = CreateCategoryHeader(__instance, SettingsTab, "TabGroup.Banlist");
            SpamlistCategoryHeader = CreateCategoryHeader(__instance, SettingsTab, "TabGroup.Spam");
            WordlistCategoryHeader = CreateCategoryHeader(__instance, SettingsTab, "TabGroup.Word");

            // 各設定スイッチを作成
            var template = __instance.GameSettingsTab.stringOptionOrigin;
            var scOptions = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
            foreach (var option in OptionItem.AllOptions)
            {
                if (option.OptionBehaviour == null)
                {
                    var stringOption = Object.Instantiate(template, SettingsTab.settingsContainer);
                    scOptions.Add(stringOption);
                    stringOption.SetClickMask(__instance.GameSettingsButton.ClickMask);
                    stringOption.SetUpFromData(stringOption.data, GameOptionsMenu.MASK_LAYER);
                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                    stringOption.TitleText.text = option.Name;
                    stringOption.Value = stringOption.oldValue = option.CurrentValue;
                    stringOption.ValueText.text = option.GetString();
                    stringOption.name = option.Name;

                    // タイトルの枠をデカくする
                    var indent = 0f;  // 親オプションがある場合枠の左を削ってインデントに見せる
                    var parent = option.Parent;
                    while (parent != null)
                    {
                        indent += 0.15f;
                        parent = parent.Parent;
                    }
                    stringOption.LabelBackground.size += new Vector2(2f - indent * 2, 0f);
                    stringOption.LabelBackground.transform.localPosition += new Vector3(-1f + indent, 0f, 0f);
                    stringOption.TitleText.rectTransform.sizeDelta += new Vector2(2f - indent * 2, 0f);
                    stringOption.TitleText.transform.localPosition += new Vector3(-1f + indent, 0f, 0f);

                    option.OptionBehaviour = stringOption;
                }
                option.OptionBehaviour.gameObject.SetActive(true);
            }
            SettingsTab.Children = scOptions;
            SettingsTab.gameObject.SetActive(false);

        }

        private const float JumpButtonSpacing = 0.6f;
        // ジャンプしたカテゴリヘッダのScrollerとの相対Y座標がこの値になる
        private const float CategoryJumpY = 2f;
        private static CategoryHeaderMasked CreateCategoryHeader(GameSettingMenu __instance, GameOptionsMenu tohTab, string translationKey)
        {
            var categoryHeader = Object.Instantiate(__instance.GameSettingsTab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, tohTab.settingsContainer);
            categoryHeader.name = translationKey;
            categoryHeader.Title.text = GetString(translationKey);
            var maskLayer = GameOptionsMenu.MASK_LAYER;
            categoryHeader.Background.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
            if (categoryHeader.Divider != null)
            {
                categoryHeader.Divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
            }
            categoryHeader.Title.fontMaterial.SetFloat("_StencilComp", 3f);
            categoryHeader.Title.fontMaterial.SetFloat("_Stencil", (float)maskLayer);
            categoryHeader.transform.localScale = Vector3.one * GameOptionsMenu.HEADER_SCALE;
            return categoryHeader;
        }

        // 初めてロール設定を表示したときに発生する例外(バニラバグ)の影響を回避するためPrefix
        [HarmonyPatch(nameof(GameSettingMenu.ChangeTab)), HarmonyPrefix]
        public static void ChangeTabPrefix(bool previewOnly)
        {
            if (!previewOnly)
            {
                if (SettingsTab)
                {
                    SettingsTab.gameObject.SetActive(false);
                }
                if (SettingsButton)
                {
                    SettingsButton.SelectButton(false);
                }
            }
        }

        public const string MenuName = "ModTab";
    }



    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public class GameOptionsMenuUpdatePatch
    {
        private static float _timer = 1f;

        public static void Postfix(GameOptionsMenu __instance)
        {
            if (__instance.name != GameSettingMenuPatch.MenuName) return;

            _timer += Time.deltaTime;
            if (_timer < 0.1f) return;
            _timer = 0f;

            var offset = 2.7f;
            var isOdd = true;

            UpdateCategoryHeader(GameSettingMenuPatch.BanlistCategoryHeader, ref offset);
            foreach (var option in OptionItem.BanlistOptions)
            {
                UpdateOption(ref isOdd, option, ref offset);
            }
            UpdateCategoryHeader(GameSettingMenuPatch.SpamlistCategoryHeader, ref offset);
            foreach (var option in OptionItem.SpamlistOptions)
            {
                UpdateOption(ref isOdd, option, ref offset);
            }
            UpdateCategoryHeader(GameSettingMenuPatch.WordlistCategoryHeader, ref offset);
            foreach (var option in OptionItem.WordlistOptions)
            {
                UpdateOption(ref isOdd, option, ref offset);
            }

            __instance.scrollBar.ContentYBounds.max = (-offset) - 1.5f;
        }
        private static void UpdateCategoryHeader(CategoryHeaderMasked categoryHeader, ref float offset)
        {
            offset -= GameOptionsMenu.HEADER_HEIGHT;
            categoryHeader.transform.localPosition = new(GameOptionsMenu.HEADER_X, offset, -2f);
        }
        private static void UpdateOption(ref bool isOdd, OptionItem item, ref float offset)
        {
            if (item?.OptionBehaviour == null || item.OptionBehaviour.gameObject == null) return;

            var enabled = true;
            var parent = item.Parent;

            // 親オプションの値を見て表示するか決める
            enabled = AmongUsClient.Instance.AmHost;
            var stringOption = item.OptionBehaviour;
            while (parent != null && enabled)
            {
                enabled = parent.GetBool();
                parent = parent.Parent;
            }

            item.OptionBehaviour.gameObject.SetActive(enabled);
            if (enabled)
            {
                // 見やすさのため交互に色を変える

                offset -= GameOptionsMenu.SPACING_Y;
                if (item.IsHeader)
                {
                    // IsHeaderなら隙間を広くする
                    offset -= HeaderSpacingY;
                }
                item.OptionBehaviour.transform.localPosition = new Vector3(
                    GameOptionsMenu.START_POS_X,
                    offset,
                    -2f);

                isOdd = !isOdd;
            }
        }

        private const float HeaderSpacingY = 0.2f;
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    public class StringOptionInitializePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.GetName();
            __instance.Value = __instance.oldValue = option.CurrentValue;
            __instance.ValueText.text = option.GetString();

            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
            if (option == null) return true;

            option.SetValue(option.CurrentValue + (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5 : 1));
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
            if (option == null) return true;

            option.SetValue(option.CurrentValue - (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5 : 1));
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            OptionItem.SyncAllOptions();
        }
    }


}
