using System;

using HarmonyLib;

using RhythmRift;
using RhythmRift.Traps;

namespace RiftAllseer {
    class GameplayPatches {
        [HarmonyPatch(typeof(RRTrapController), "SpawnTrap")]
        [HarmonyPrefix]
        static bool SkipAllTrapSpawns(ref TrapSpawnData trapSpawnData) {
            if (trapSpawnData.TrapType == RRTrapType.Mystery && Plugin.shouldDisableHiddenTraps.Value)
            {
                Console.WriteLine("Skipping ? trap spawned");
                return false; // Skip original method
            }
            return true; // Execute original method
        }

        [HarmonyPatch(typeof(RRStageController), "UploadScoreToLeaderboardAndRefreshUi")]
        [HarmonyPrefix]
        static bool DisableLeaderboardScores(){
            Console.WriteLine("B Preventing score upload.");  // TODO make this more player visible
            return false;
        }
    }
}