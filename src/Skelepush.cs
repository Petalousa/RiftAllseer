
using System;
using System.Reflection;
using HarmonyLib;
using Shared;
using Shared.Title;
using Shared.PlayerData;
using RhythmRift.Enemies;
using UnityEngine;

namespace RiftAllseer {
    public class Skelepush{
        [HarmonyPatch(typeof(RRSkeletonEnemy), "ProcessIncomingAttack")]
        [HarmonyPostfix]
        public static void kickSkeleton(ref RRSkeletonEnemy __instance){
            // if instance is dead do nothing.
            // if instance is alive and moving backwards, flip sprite
            // if instance is alive but still approaching, kick

            Console.Write("a");

            bool _isRetreating = Plugin.GetValue<bool>(typeof(RRSkeletonEnemy), __instance, "_isRetreating");
            Console.Write("b");

            if (_isRetreating){
                Plugin.GetMethod(typeof(RRSkeletonEnemy), "FlipHorizontally").Invoke(__instance, []);
            }

            if (Plugin.GetValue<bool>(typeof(RRSkeletonEnemy), __instance, "_wasJustHitHoldingShield")){
                Vector3 newPosition = __instance.transform.position;
                newPosition.z += 0.5f;
                __instance.transform.position = newPosition;
                // +y = closer to screen, -y = further from screen
                // +z = closer up screen, -z = closer down screen

                // TODO need 
            }
        }

        [HarmonyPatch(typeof(RRSkeletonEnemy), "PerformCollisionResponse")]
        [HarmonyPrefix]
        public static bool unflip(ref RRSkeletonEnemy __instance, ref bool shouldForceDestruction){
            bool _isHeadless = Plugin.GetValue<bool>(typeof(RRSkeletonEnemy), __instance, "_isHeadless");
            bool _isRetreating = Plugin.GetValue<bool>(typeof(RRSkeletonEnemy), __instance, "_isRetreating");

            if (_isHeadless && _isRetreating && !shouldForceDestruction)
            {
                Plugin.GetMethod(typeof(RRSkeletonEnemy), "FlipHorizontally").Invoke(__instance, []);
            }
            return true;
        }

    }
}