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
using System.Reflection;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace RiftAllseer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("RiftOfTheNecroDancer.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static ConfigEntry<bool> shouldDisableAnalytics;
    internal static ConfigEntry<bool> shouldLogAnalytics;

    internal static ConfigEntry<bool> shouldDisableHiddenTraps;

    internal static ConfigEntry<float> scrollSpeedModifier;
    internal static float z_offset = 0.0f;
    internal static float y_offset = 0.0f;

    private GameObject _textObject;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        scrollSpeedModifier = Config.Bind("Gameplay", "Scroll Speed Modifier", 1.0f, "Increase / Decrease Scroll Speed");
        shouldDisableHiddenTraps = Config.Bind("Traps", "Disable Hidden Traps", true, "Prevents ? traps from spawning.");

        shouldDisableAnalytics = Config.Bind("Analytics", "Disable Analytics", true, "Prevents Analytics from being sent.");
        shouldLogAnalytics = Config.Bind("Analytics", "Log Analytics to Console", false, "Shows JSON of analytics in console.");
        // TODO allow player to change this and reload while in game instead of requiring restart.

        shouldDisableHiddenTraps.SettingChanged += (sender, args) => 
        {
            var configEntry = (ConfigEntry<bool>)sender;
            // Log the new value when changed
            Logger.LogInfo($"Config changed: {configEntry.Value}");
        };
        scrollSpeedModifier.SettingChanged += (sender, args) => 
        {
            var configEntry = (ConfigEntry<bool>)sender;
            Logger.LogInfo($"scrollSpeedModifier changed: {configEntry.Value}");
        };

        Harmony.CreateAndPatchAll(typeof(Plugin));

        // Positioning (using anchors for top-right)
        RectTransform rectTransform = _textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1); // Top-right
        rectTransform.anchorMax = new Vector2(1, 1); // Top-right
        rectTransform.pivot = new Vector2(1, 1);   // Top-right
        rectTransform.anchoredPosition = new Vector2(-10, -10); // Offset from top-right corner (adjust as needed)


        Logger.LogInfo("Patched");
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.F5)) // Press F5 to reload config
        {
            Config.Reload();
            Logger.LogInfo("Config reloaded manually.A");
            Logger.LogWarning($"shouldDisableHiddenTraps {shouldDisableHiddenTraps.Value}");
            Logger.LogWarning($"shouldDisableAnalytics {shouldDisableAnalytics.Value}");
            Logger.LogWarning($"shouldLogAnalytics {shouldLogAnalytics.Value}");
            Logger.LogWarning($"scrollSpeedModifier {scrollSpeedModifier.Value}");
        }

        // use page up / down to modify scroll speed.
        if (Input.GetKeyDown(KeyCode.LeftShift)){
            if (Input.GetKeyUp(KeyCode.PageUp))
            {
                z_offset += 0.1f;
            }
            if (Input.GetKeyUp(KeyCode.PageDown))
            {
                z_offset -= 0.1f;
            }
        } else {
            if (Input.GetKeyUp(KeyCode.PageUp))
            {
                y_offset += 0.1f;
            }
            if (Input.GetKeyUp(KeyCode.PageDown))
            {
                y_offset -= 0.1f;
            }
        }
        if (Input.GetKeyUp(KeyCode.Home))
        {
            Logger.LogWarning($"offsets info: y {y_offset} z {z_offset}");
        }
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
        Console.WriteLine("Preventing score upload.");  // TODO make this more player visible
        return false;
    }

    [HarmonyPatch(typeof(RhythmRift.RRTileView), "Awake")]
    [HarmonyPostfix]
    static void CreateTile(ref MaterialPropertyBlock ____materialPropertyBlock){
        Console.WriteLine($"Tile AWAKE! {____materialPropertyBlock}");
        Vector3 newPosition = ____materialPropertyBlock.GetVector("_Position");
        newPosition.x -= 100.0f;
        newPosition.y += 500.0f;
        newPosition.z += 700.0f;
        ____materialPropertyBlock.SetVector("_Position", newPosition);
    }

    [HarmonyPatch(typeof(RhythmRift.RRGridView), "GetTileWorldPositionFromGridPosition")]
    [HarmonyPostfix]
    static void GetTileWorldPositionFromGridPosition(int xCoordinate, int yCoordinate, ref Vector3 __result){
        //Console.WriteLine($"modifying coord for {xCoordinate} {yCoordinate} - {__result.x} {__result.y} {__result.z}!");
        __result = new Vector3(
            __result.x ,//+ (float)Math.Sin(Time.time * 4.0) * 0.5f,
            __result.y + y_offset, // y is near/far plane (can use it for shrinking lmao)
            __result.z + z_offset  // z is up/down screen
        );
    }

    // [HarmonyPatch(typeof(RhythmRift.RREnemyInitializationData), "SetData")]
    // [HarmonyPrefix]
    // static bool SetData(
    //     ref RREnemyDefinition enemyDefinition,
    //     bool shouldStartFacingRight, float spawnTrueBeatNumber,
    //     int2 gridPosition, Vector3 worldPosition, int enemyLength,
    //     Guid groupId, int itemToDropOnDeathId, bool shouldIgnoreForTutorialSuccess,
    //     bool shouldClampToSubdivisions = true
	// ) {
    //     Console.WriteLine($"setenemy! {enemyDefinition.DisplayName} {worldPosition}");
    //     return true;
    // }


    [HarmonyPatch(typeof(Shared.RhythmEngine.BeatmapPlayer), "SetSongSpeedModifier")]
    [HarmonyPrefix]
    static bool ForceDoubleTime(ref BeatmapPlayer __instance){
        Console.WriteLine("Forcing Double Time :)");
        typeof(BeatmapPlayer).GetField("_activeSpeedAdjustment", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, 2.0d);

        return true;
    }

    [HarmonyPatch(typeof(Shared.RhythmEngine.BeatmapPlayer), "Awake")]
    [HarmonyPrefix]
    static void Awake(ref BeatmapPlayer __instance){
        Console.WriteLine("Awaken and BIND!");
        const double newSpeed = 0.5d;
        typeof(BeatmapPlayer).GetField("_activeSpeedAdjustment", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, newSpeed);

        // Or, if you know the field's type:
        // __instance.GetType().GetField("yourPrivateVariableName", BindingFlags.Instance | BindingFlags.NonPublic).Se
    }

    [HarmonyPatch(typeof(Shared.RhythmEngine.BeatmapPlayer), "SetBeatmapInternal")]
    [HarmonyPrefix]
    static bool SetBeatmapInternal(ref Beatmap beatmapToSet){
        Console.WriteLine("overrideing internal bpm speed.");
        beatmapToSet.bpm *= 2;
        return true;
    }

    //     Awake()
}