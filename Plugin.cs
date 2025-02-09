using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;
using HarmonyLib;
using RhythmRift.Traps;
using RhythmRift;
using Shared.RhythmEngine;
using Shared.Analytics;
using System;


namespace RiftAllseer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("RiftOfTheNecroDancer.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static ConfigEntry<bool> shouldDisableAnalytics;
    internal static ConfigEntry<bool> shouldLogAnalytics;

    internal static ConfigEntry<bool> shouldDisableHiddenTraps;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        shouldDisableHiddenTraps = Config.Bind("Traps", "Disable Hidden Traps", true, "Prevents ? traps from spawning.");

        shouldDisableAnalytics = Config.Bind("Analytics", "Disable Analytics", true, "Prevents Analytics from being sent.");
        shouldLogAnalytics = Config.Bind("Analytics", "Log Analytics to Console", false, "Shows JSON of analytics in console.");

        Harmony.CreateAndPatchAll(typeof(Plugin));
        Logger.LogInfo("Patched");
    }

    [HarmonyPatch(typeof(RRTrapController), "SpawnTrap")]
    [HarmonyPrefix]
    static bool SkipAllTrapSpawns(ref TrapSpawnData trapSpawnData) {
        if (trapSpawnData.TrapType == RRTrapType.Mystery && shouldDisableHiddenTraps.Value)
        {
            Console.WriteLine("Skipping ? trap spawned");
            return false; // Skip original method
        }
        return true; // Execute original method
    }

    [HarmonyPatch(typeof(RREnemyController), "SpawnEnemy", new Type[] {typeof(SpawnEnemyData), typeof(Guid), typeof(FmodTimeCapsule)})]
    [HarmonyPrefix]
    static bool AlertEnemySpawn() {
        //Console.WriteLine("enemy spawned");
        return true; // Execute original method
    }

    [HarmonyPatch(typeof(RiftAnalyticsService), "SendAnalyticsEvent")]
    [HarmonyPrefix]
    static bool DisableAnalytics(ref string tableName, ref string dataJsonString, bool shouldLogResponse = true){
        if (shouldLogAnalytics.Value) {
            Console.Write("Analytics send event triggered:");
            Console.WriteLine(tableName);
            Console.WriteLine(dataJsonString);
        }

        if (shouldDisableAnalytics.Value){
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(RRStageController), "UploadScoreToLeaderboardAndRefreshUi")]
    [HarmonyPrefix]
    static bool DisableLeaderboardScores(){
        if (shouldDisableHiddenTraps.Value){
            Console.WriteLine("Preventing score upload.");
            return false;
        }
        return true;
    }

}