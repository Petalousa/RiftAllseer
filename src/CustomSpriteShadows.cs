using System.Reflection;

using HarmonyLib;
using RhythmRift.Enemies;
using UnityEngine;

namespace RiftAllseer
{
    class CustomSpriteShadows{

        internal static void LoadSprites()
        {
            SpriteLoader.LoadSprite("on_beat.PNG", "onBeatShadow", 512, 512);
            SpriteLoader.LoadSprite("half_beat.PNG", "halfBeatShadow", 512, 512);
            SpriteLoader.LoadSprite("other_beat.PNG", "otherBeatShadow", 512, 512);
        }

        [HarmonyPatch(typeof(RREnemy), "Initialize")]
        [HarmonyPrefix]
        static bool PatchEnemyShadowSprites(ref RREnemy __instance){
            Sprite onBeatShadowSprite = SpriteLoader.GetSprite("onBeatShadow");
            Sprite halfBeatShadowSprite = SpriteLoader.GetSprite("halfBeatShadow");
            Sprite otherBeatShadowSprite = SpriteLoader.GetSprite("otherBeatShadow");

            if (onBeatShadowSprite != null){
                FieldInfo onBeatField = typeof(RREnemy).GetField("_onBeatShadowSprite", BindingFlags.Instance | BindingFlags.NonPublic);
                onBeatField.SetValue(__instance, onBeatShadowSprite);
            }
            if (onBeatShadowSprite != null){
                FieldInfo halfBeatField = typeof(RREnemy).GetField("_halfBeatShadowSprite", BindingFlags.Instance | BindingFlags.NonPublic);
                halfBeatField.SetValue(__instance, halfBeatShadowSprite);
            }
            if (onBeatShadowSprite != null){
                FieldInfo otherBeatField = typeof(RREnemy).GetField("_otherBeatShadowSprite", BindingFlags.Instance | BindingFlags.NonPublic);
                otherBeatField.SetValue(__instance, otherBeatShadowSprite);
            }

            return true;
        }
    }
}