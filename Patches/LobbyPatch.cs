using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Rewired.Utils.Platforms.Windows;
using TMPro;
using UnityEngine;

namespace BanMod;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public class LobbyStartPatch
{
    private static GameObject LobbyPaintObject;
    private static GameObject DecorationsObject;
    private static Sprite LobbyPaintSprite;
    private static Sprite DropshipDecorationsSprite;

    public static void Prefix()
    {
        LobbyPaintSprite = Utils.LoadSprite("BanMod.Resources.LobbyPaint.png", 290f);
        DropshipDecorationsSprite = Utils.LoadSprite("BanMod.Resources.Decoration.png", 60f);
    }
    public static void Postfix(LobbyBehaviour __instance)
    {
        __instance.StartCoroutine(CoLoadDecorations().WrapToIl2Cpp());

        static System.Collections.IEnumerator CoLoadDecorations()
        {
            var LeftBox = GameObject.Find("Leftbox");
            if (LeftBox != null)
            {
                LobbyPaintObject = Object.Instantiate(LeftBox, LeftBox.transform.parent.transform);
                LobbyPaintObject.name = "Lobby Paint";
                LobbyPaintObject.transform.localPosition = new Vector3(0.042f, -2.59f, -10.5f);
                SpriteRenderer renderer = LobbyPaintObject.GetComponent<SpriteRenderer>();
                renderer.sprite = LobbyPaintSprite;
            }

            yield return null;

            if (BanMod.AktiveLobby.Value)
            {
                var Dropship = GameObject.Find("SmallBox");
                if (Dropship != null)
                {
                    DecorationsObject = Object.Instantiate(Dropship, Object.FindAnyObjectByType<LobbyBehaviour>().transform);
                    DecorationsObject.name = "Lobby_Decorations";
                    DecorationsObject.transform.DestroyChildren();
                    Object.Destroy(DecorationsObject.GetComponent<PolygonCollider2D>());
                    DecorationsObject.GetComponent<SpriteRenderer>().sprite = DropshipDecorationsSprite;
                    DecorationsObject.transform.SetSiblingIndex(1);
                    DecorationsObject.transform.localPosition = new(0.05f, 0.8334f);
                }
            }

            yield return null;

        }
    }

}
// https://github.com/SuperNewRoles/SuperNewRoles/blob/master/SuperNewRoles/Patches/LobbyBehaviourPatch.cs
[HarmonyPatch(typeof(LobbyBehaviour))]
public class LobbyBehaviourPatch
{
    [HarmonyPatch(nameof(LobbyBehaviour.Update)), HarmonyPostfix]
    public static void Update_Postfix(LobbyBehaviour __instance)
    {
        System.Func<ISoundPlayer, bool> lobbybgm = x => x.Name.Equals("MapTheme");
        ISoundPlayer MapThemeSound = SoundManager.Instance.soundPlayers.Find(lobbybgm);
        if (BanMod.DisableLobbyMusic.Value)
        {
            if (MapThemeSound == null) return;
            SoundManager.Instance.StopNamedSound("MapTheme");
        }
        else
        {
            if (MapThemeSound != null) return;
            SoundManager.Instance.CrossFadeSound("MapTheme", __instance.MapTheme, 0.5f);
        }
    }
}